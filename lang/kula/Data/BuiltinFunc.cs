using System;
using System.Collections.Generic;

using Kula.Core;
using Kula.Util;

namespace Kula.Data
{
    /// <summary>
    /// 内置函数对应委托 添加扩展函数时需要实现之
    /// </summary>
    /// <param name="args">参数</param>
    /// <param name="engine">对应Kula引擎</param>
    /// <returns>返回值</returns>
    public delegate object BFunc(object[] args, KulaEngine engine);

    /// <summary>
    /// 内置变量对应委托 
    /// </summary>
    /// <param name="engine">对应Kula引擎</param>
    /// <returns>变量对应值</returns>
    public delegate object BVal(KulaEngine engine);

    /// <summary>
    /// 内置函数类
    /// </summary>
    public class BuiltinFunc
    {
        /// <summary>
        /// Kula 内置方法表
        /// </summary>
        public static Dictionary<string, BFunc> BFuncs { get; } = new Dictionary<string, BFunc>()
        {
            // Num
            {"plus", (args, engine) => {
                ArgsCheck(args, typeof(float), typeof(float));
                return((float)args[0] + (float)args[1]);
            } },
            {"minus", (args, engine) => {
                ArgsCheck(args, typeof(float), typeof(float));
                return((float)args[0] - (float)args[1]);
            } },
            {"times", (args, engine) => {
                ArgsCheck(args, typeof(float), typeof(float));
                return((float)args[0] * (float)args[1]);
            } },
            {"div", (args, engine) => {
                ArgsCheck(args, typeof(float), typeof(float));
                return((float)args[0] / (float)args[1]);
            } },
            {"floor", (args, engine) => {
                ArgsCheck(args, typeof(float));
                return( (float)Math.Floor((float)args[0]) );
            } },
            {"mod", (args, engine) => {
                ArgsCheck(args, typeof(float), typeof(float));
                return( (float)((int)(float)args[0] % (int)(float)args[1]) );
            } },

            // IO
            {"print", (args, engine) => {
                foreach (var arg in args)
                {
                    Console.Write( arg.ToString() );
                }
                return null;
            } },
            {"println", (args, engine) => {
                foreach (var arg in args)
                {
                    Console.Write( arg.ToString() );
                }
                Console.WriteLine();
                return null;
            } },
            {"input", (args, engine) =>{
                return(Console.ReadLine());
            } },
            
            // String
            {"toStr", (args, engine) => {
                var ret = args[0] is BFunc ? Parser.InvertTypeDict[typeof(BFunc)] : args[0].ToString();
                return(ret);
            } },
            {"parseNum", (args, engine) => {
                var arg = args[0];
                ArgsCheck(args, typeof(string));
                float.TryParse((string)arg, out float ans);
                return(ans);
            } },
            {"len", (args, engine) => {
                ArgsCheck(args, typeof(string));
                return((float)((string)args[0]).Length);
            } },
            {"cut", (args, engine) => {
                ArgsCheck(args, typeof(string), typeof(float), typeof(float));
                return(((string)args[0]).Substring((int)(float)args[1], (int)(float)args[2]));
            } },
            {"concat", (args, engine) => {
                ArgsCheck(args, typeof(string), typeof(string));
                return((string)args[0] + (string)args[1]);
            } },
            {"type", (args, engine) => {
                if (args[0] == null)
                {
                    return("None");
                }
                return args[0].GetType().KTypeToString();
            } },

            // Bool
            {
                "equal",
                (args, engine) => {
                    return( object.Equals(args[0], args[1]) ? 1f : 0f);
                }
            },
            {"greater", (args, engine) => {
                ArgsCheck(args, typeof(float), typeof(float));
                return( ((float)args[0] > (float)args[1]) ? 1f : 0f);
            } },
            {"less",  (args, engine) => {
                ArgsCheck(args, typeof(float), typeof(float));
                return( ((float)args[0] < (float)args[1]) ? 1f : 0f);
            } },
            {"and",  (args, engine) => {
                ArgsCheck(args, typeof(float), typeof(float));
                bool flag = ((float)args[0] != 0) && ((float)args[1] != 0);
                return(flag ? 1f : 0f);
            } },
            {"or", (args, engine) => {
                ArgsCheck(args, typeof(float), typeof(float));
                bool flag = ((float)args[0] != 0) || ((float)args[1] != 0);
                return(flag ? 1f : 0f);
            } },
            {"not",  (args, engine) => {
                ArgsCheck(args, typeof(float));
                return((float)args[0] == 0f ? 1f : 0f);
            } },

            // Array
            {"newArray", (args, engine) => {
                ArgsCheck(args, typeof(float));
                Array tmp = new Array((int)(float)args[0]);
                return(tmp);
            } },
            {"fill", (args, engine) => {
                ArgsCheck(args, typeof(Array), typeof(float), typeof(object));
                ((Array)args[0]).Data[(int)(float)args[1]] = args[2];
                return null;
            } },
            {"size", (args, engine) => {
                ArgsCheck(args, typeof(Array));
                return((float) ((Array)args[0]).Data.Length);
            } },

            // Map
            {"newMap", (args, engine) =>{
                Map tmp_map = new Map();
                return(tmp_map);
            } },
            {"put", (args, engine) => {
                ArgsCheck(args, typeof(Map), typeof(string), typeof(object));
                ((Map)args[0]).Data[(string)args[1]] = args[2];
                return null;
            } },
            {"remove", (args, engine) => {
                ArgsCheck(args, typeof(Map), typeof(string));
                ((Map)args[0]).Data.Remove((string)args[1]);
                return null;
            } },
            {"count", (args, engine) => {
                ArgsCheck(args, typeof(Map));
                return((float) ((Map)args[0]).Data.Count);
            } },
            {"keyIn", (args, engine) => {
                ArgsCheck(args, typeof(Map), typeof(string));
                return((float) (((Map)args[0]).Data.ContainsKey((string)args[1]) ? 1f : 0f ));
            } },
            {"for", (args, engine) =>
            {
                ArgsCheck(args, typeof(Map), typeof(FuncWithEnv));
                foreach(var kv in ((Map)args[0]).Data)
                {
                    new FuncRuntime((FuncWithEnv)args[1], engine).Run(new object[2] { kv.Key, kv.Value });
                }
                return null;
            } },

            // ASCII
            {"AsciiToChar", (args, engine) => {
                ArgsCheck(args, typeof(float));
                return(((char)(int)(float)args[0]).ToString());
            } },
            {"CharToAscii", (args, engine) => {
                ArgsCheck(args, typeof(string));
                if (!float.TryParse((string)args[0], out float tmp)) { tmp = 0; }
                return(tmp);
            } },


            // Exception
            {"throw", (args, engine) => {
                ArgsCheck(args, typeof(string));
                throw new KulaException.UserException((string)args[0]);
            } },
        };

        /// <summary>
        /// Kula 内部变量表
        /// </summary>
        public static Dictionary<string, BVal> BVals { get; } = new Dictionary<string, BVal>()
        {
            {"null", (engine) => {
                return null;
            } },
            {"dataMap", (engine) => {
                return engine.DataMap;
            } }
        };


        /// <summary>
        /// 类型断言
        /// </summary>
        /// <param name="args">参数数组</param>
        /// <param name="types">类型数组</param>
        public static void ArgsCheck(object[] args, params Type[] types)
        {
            bool flag = args.Length == types.Length;
            for (int i = 0; i < args.Length && flag; i++)
            {
                flag = types[i] == typeof(object) || args[i].GetType() == types[i];
                if (!flag) throw new KulaException.ArgsTypeException(args[i].GetType().Name, types[i].Name);
            }
        }
    }
}
