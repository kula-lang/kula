using Kula.Core;
using Kula.Data.Container;
using Kula.Data.Type;
using Kula.Util;
using Kula.Xception;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kula.Data.Function
{
    /// <summary>
    /// 内置函数对应委托 添加扩展函数时需要实现之
    /// </summary>
    /// <param name="args">参数</param>
    /// <param name="engine">对应Kula引擎</param>
    /// <returns>返回值</returns>
    public delegate object SharpLambda(object[] args, KulaEngine engine);

    /// <summary>
    /// 内置变量对应委托 
    /// </summary>
    /// <param name="engine">对应Kula引擎</param>
    /// <returns>变量对应值</returns>
    public delegate object SharpValue(KulaEngine engine);

    /// <summary>
    /// 内置函数类
    /// </summary>
    public class SharpFunc
    {
        private SharpLambda Lambda { get; set; }
        private IType[] Types { get; set; }

        private bool ToBeChecked { get; set; }

        public SharpFunc(SharpLambda lambda, params IType[] types)
        {
            Lambda = lambda;
            Types = types;
            ToBeChecked = true;
        }

        public SharpFunc(SharpLambda lambda, bool toBeChecked)
        {
            Lambda = lambda;
            Types = (ToBeChecked = toBeChecked) ? new IType[0] : null;
        }



        public object Run(object[] args, KulaEngine engine)
        {
            if (ToBeChecked)
            {
                ArgsCheckIType(args, engine.CheckMode(KulaEngine.Config.TYPE_CHECK), Types);
            }
            return Lambda(args, engine);
        }

        /// <summary>
        /// Kula 内置方法表
        /// </summary>
        public static Dictionary<string, SharpFunc> SharpFuncs { get; } = new Dictionary<string, SharpFunc>()
        {
            // Core
            ["plus"] = new SharpFunc((args, engine) => (float)args[0] + (float)args[1], RawType.Num, RawType.Num),
            ["minus"] = new SharpFunc((args, engine) => (float)args[0] - (float)args[1], RawType.Num, RawType.Num),
            ["times"] = new SharpFunc((args, engine) => (float)args[0] * (float)args[1], RawType.Num, RawType.Num),
            ["div"] = new SharpFunc((args, engine) => (float)args[0] / (float)args[1], RawType.Num, RawType.Num),

            // String
            ["type"] = new SharpFunc((args, engine) =>
            {
                object arg = args[0];
                if (arg == null)
                    return RawType.None.ToString();
                foreach (RawType type in RawType.TypeDict.Values)
                {
                    if (type == RawType.Any)
                        continue;
                    if (type.Check(arg))
                        return type.ToString();
                }
                return new KTypeException(arg.GetType().Name);
            }, RawType.Any),

            // Bool
            ["eq"] = new SharpFunc((args, engine) => Equals(args[0], args[1]) ? 1f : 0f, RawType.Any, RawType.Any),
            ["gt"] = new SharpFunc((args, engine) => (float)args[0] > (float)args[1] ? 1f : 0f, RawType.Num, RawType.Num),
            ["lt"] = new SharpFunc((args, engine) => (float)args[0] < (float)args[1] ? 1f : 0f, RawType.Num, RawType.Num),
            ["and"] = new SharpFunc((args, engine) => ((float)args[0] != 0f) && ((float)args[0] != 0f) ? 1f : 0f, RawType.Num, RawType.Num),
            ["or"] = new SharpFunc((args, engine) => ((float)args[0] != 0f) || ((float)args[0] != 0f) ? 1f : 0f, RawType.Num, RawType.Num),
            ["not"] = new SharpFunc((args, engine) => (float)args[0] == 0f ? 1f : 0f, RawType.Num),

            // Throw
            ["throw"] = new SharpFunc((args, engine) => throw new UserException((string)args[0]), RawType.Str),

            // Unpack
            ["unpack"] = new SharpFunc((args, engine) =>
            {
                var map = ((Map)args[0]).Data;

                foreach (var kvp in map)
                    if (kvp.Value is SharpFunc bfunc)
                        if (kvp.Key == "new")
                            engine.ExtendFunc["new" + (string)map["namespace"]] = bfunc;
                        else
                            engine.ExtendFunc[kvp.Key] = bfunc;
                return null;
            }, RawType.Map),

            ["unpackAll"] = new SharpFunc((args, engine) =>
            {
                foreach (var bval in SharpVals)
                    if (bval.Value(engine) is Map name_space && name_space.Data.ContainsKey("namespace"))
                        foreach (var func in name_space.Data)
                            if (func.Value is SharpFunc func_value)
                                engine.ExtendFunc[func.Key == "new" ? ("new" + bval.Key) : func.Key] = func_value;
                return null;
            }),
        };

        private static readonly Map
            math = new Map(),
            shell = new Map(),
            str = new Map(),
            map = new Map(),
            ascii = new Map(),
            array = new Map();

        static SharpFunc()
        {
            SharpFuncs["+"] = SharpFuncs["plus"];
            SharpFuncs["-"] = SharpFuncs["minus"];
            SharpFuncs["*"] = SharpFuncs["times"];
            SharpFuncs["/"] = SharpFuncs["div"];

            // math
            math.Data["namespace"] = "Math";
            math.Data["floor"] = new SharpFunc((args, engine) => (float)Math.Floor((float)args[0]), RawType.Num);
            math.Data["mod"] = new SharpFunc(
                (args, engine) => (float)((int)(float)args[0] % (int)(float)args[1]),
                RawType.Num, RawType.Num);
            math.Data["abs"] = new SharpFunc((args, engine) => (float)Math.Abs((float)args[0]), RawType.Num);

            // IO
            shell.Data["namespace"] = "Shell";
            shell.Data["println"] = new SharpFunc((args, engine) =>
            {
                foreach (var arg in args) { Console.Write(arg.ToString()); }
                Console.WriteLine();
                return null;
            }, false);
            shell.Data["print"] = new SharpFunc((args, engine) =>
            {
                foreach (var arg in args) { Console.Write(arg.ToString()); }
                return null;
            }, false);
            shell.Data["input"] = new SharpFunc((args, engine) =>
            {
                return Console.ReadLine();
            }, true);

            // Str
            str.Data["namespace"] = "Str";
            str.Data["toStr"] = new SharpFunc((args, engine) =>
            {
                var ret = args[0] is SharpFunc ? RawType.InvertTypeDict[RawType.SharpFunc] : args[0].ToString();
                return ret;
            }, RawType.Any);
            str.Data["parseNum"] = new SharpFunc((args, engine) => {
                if (float.TryParse((string)args[0], out float ret))
                    return ret;
                return (string)args[0];
            }, RawType.Str);
            str.Data["len"] = new SharpFunc((args, engine) => (float)((string)args[0]).Length, RawType.Str);
            str.Data["cut"] = new SharpFunc((args, engine) =>
            {
                return ((string)args[0]).Substring((int)(float)args[1], (int)(float)args[2]);
            }, RawType.Str, RawType.Num, RawType.Num);
            str.Data["concat"] = new SharpFunc((args, engine) =>
            {
                return (string)args[0] + (string)args[1];
            }, RawType.Str, RawType.Str);
            str.Data["charAt"] = new SharpFunc((args, engine) =>
            {
                return ((string)args[0])[(int)(float)args[1]].ToString();
            }, RawType.Str, RawType.Num);

            // ASCII
            ascii.Data["namespace"] = "ASCII";
            ascii.Data["atoc"] = new SharpFunc((args, engine) =>
            {
                return ((char)(int)(float)args[0]).ToString();
            }, RawType.Num);
            ascii.Data["ctoa"] = new SharpFunc((args, engine) =>
            {
                return (float)(int)((string)args[0])[0];
            }, RawType.Str);

            // Array
            array.Data["namespace"] = "Array";
            array.Data["new"] = new SharpFunc((args, engine) =>
            {
                Container.Array tmp = new Container.Array((int)(float)args[0]);
                return tmp;
            }, RawType.Num);
            array.Data["fill"] = new SharpFunc((args, engine) =>
            {
                ((Container.Array)args[0]).Data[(int)(float)args[1]] = args[2];
                return null;
            }, RawType.Any, RawType.Num, RawType.Any);
            array.Data["size"] = new SharpFunc((args, engine) =>
            {
                return (float)((Container.Array)args[0]).Data.Length;
            }, RawType.Array);

            // Map
            map.Data["namespace"] = "Map";
            map.Data["new"] = new SharpFunc((args, engine) =>
            {
                Map tmp_map = new Map();
                return (tmp_map);
            });
            map.Data["put"] = new SharpFunc((args, engine) =>
            {
                ((Map)args[0]).Data[(string)args[1]] = args[2];
                return null;
            }, RawType.Map, RawType.Str, RawType.Any);
            map.Data["remove"] = new SharpFunc((args, engine) =>
            {
                ((Map)args[0]).Data.Remove((string)args[1]);
                return null;
            }, RawType.Map, RawType.Str);
            map.Data["count"] = new SharpFunc((args, engine) =>
            {
                return ((float)((Map)args[0]).Data.Count);
            }, RawType.Map);
            map.Data["keyIn"] = new SharpFunc((args, engine) =>
            {
                return (float)(((Map)args[0]).Data.ContainsKey((string)args[1]) ? 1f : 0f);
            }, RawType.Map, RawType.Str);
            map.Data["for"] = new SharpFunc((args, engine) =>
            {
                foreach (var kv in ((Map)args[0]).Data)
                {
                    new FuncRuntime((Func)args[1], engine).Run(new object[2] { kv.Key, kv.Value }, 0);
                }
                return null;
            }, RawType.Map, RawType.Func);
        }

        /// <summary>
        /// Kula 内部变量表
        /// </summary>
        public static Dictionary<string, SharpValue> SharpVals { get; } = new Dictionary<string, SharpValue>()
        {
            ["null"] = (_) => null,
            ["dataMap"] = (_engine) => _engine.DataMap,
            ["Math"] = (_) => math,
            ["Map"] = (_) => map,
            ["Array"] = (_) => array,
            ["Str"] = (_) => str,
            ["Shell"] = (_) => shell,
            ["ASCII"] = (_) => ascii,
        };

        /// <summary>
        /// 类型检查
        /// </summary>
        /// <param name="args">参数数组</param>
        /// <param name="types">类型数组</param>
        public static void ArgsCheckIType(object[] args, bool needCheck, IType[] types)
        {
            bool flag = args.Length == (types == null ? 0 : types.Length); 
            if (!flag)
                throw new FuncArgumentException("SharpFunc", types, types.Length);
            if (needCheck)
            {
                for (int i = 0; i < args.Length && flag; ++i)
                    flag = types[i].Check(args[i]);
                if (!flag)
                    throw new FuncArgumentException("SharpFunc", types);
            }
        }

        private string @string;
        public override string ToString()
        {
            if (@string == null)
            {
                StringBuilder sb = new StringBuilder("func(");
                if (!ToBeChecked)
                {
                    sb.Append("...)");
                }
                else
                {
                    for(int i=0; i<Types.Length; ++i)
                    {
                        if (i != 0)
                            sb.Append(',');
                        sb.Append(Types[i].ToString());
                    }
                    sb.Append(")");
                }
                @string = sb.ToString();
            }
            return @string;
        }
    }
}
