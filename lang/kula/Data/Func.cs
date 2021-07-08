using System;
using System.Collections.Generic;

using Kula.Util;
using Kula.Core;
using System.Text;

namespace Kula.Data
{
    /// <summary>
    /// 内置函数 添加扩展函数时需要实现之
    /// </summary>
    /// <param name="args">参数</param>
    /// <param name="stack">目标栈</param>
    /// <param name="engine">对应Kula引擎</param>
    public delegate void BuiltinFunc(object[] args, Stack<object> stack, KulaEngine engine);
    class Func
    {
        // 静态
        /// <summary>
        /// Kula 内置方法表
        /// </summary>
        public static Dictionary<string, BuiltinFunc> BuiltinFunc { get => builtinFunc; }
        private readonly static Dictionary<string, BuiltinFunc> builtinFunc = new Dictionary<string, BuiltinFunc>()
        {
            // Num
            {"plus", (args, stack, engine) => {
                ArgsCheck(args, typeof(float), typeof(float));
                stack.Push((float)args[0] + (float)args[1]);
            } },
            {"minus", (args, stack, engine) => {
                ArgsCheck(args, typeof(float), typeof(float));
                stack.Push((float)args[0] - (float)args[1]);
            } },
            {"times", (args, stack, engine) => {
                ArgsCheck(args, typeof(float), typeof(float));
                stack.Push((float)args[0] * (float)args[1]);
            } },
            {"div", (args, stack, engine) => {
                ArgsCheck(args, typeof(float), typeof(float));
                stack.Push((float)args[0] / (float)args[1]);
            } },
            {"floor", (args, stack, engine) => {
                ArgsCheck(args, typeof(float));
                stack.Push( (float)Math.Floor((float)args[0]) );
            } },
            {"mod", (args, stack, engine) => {
                ArgsCheck(args, typeof(float), typeof(float));
                stack.Push( (float)((int)(float)args[0] % (int)(float)args[1]) );
            } },

            // IO
            {"print", (args, stack, engine) => {
                foreach (var arg in args)
                {
                    Console.Write( arg.ToString() );
                }
            } },
            {"println", (args, stack, engine) => {
                foreach (var arg in args)
                {
                    Console.Write( arg.ToString() );
                }
                Console.WriteLine();
            } },
            {"input", (args, stack, engine) =>{
                stack.Push(Console.ReadLine());
            } },
            
            // String
            {"toStr", (args, stack, engine) => {
                var ret = args[0] is BuiltinFunc ? Parser.InvertTypeDict[typeof(BuiltinFunc)] : args[0].ToString();
                stack.Push(ret);
            } },
            {"parseNum", (args, stack, engine) => {
                var arg = args[0];
                ArgsCheck(args, typeof(string));
                float.TryParse((string)arg, out float ans);
                stack.Push(ans);
            } },
            {"len", (args, stack, engine) => {
                ArgsCheck(args, typeof(string));
                stack.Push((float)((string)args[0]).Length);
            } },
            {"cut", (args, stack, engine) => {
                ArgsCheck(args, typeof(string), typeof(float), typeof(float));
                stack.Push(((string)args[0]).Substring((int)(float)args[1], (int)(float)args[2]));
            } },
            {"concat", (args, stack, engine) => {
                ArgsCheck(args, typeof(string), typeof(string));
                stack.Push((string)args[0] + (string)args[1]);
            } },
            {"type", (args, stack, engine) => {
                if (args[0] == null)
                {
                    stack.Push("Null");
                    return;
                }
                var arg = args[0].GetType();
                if (Parser.InvertTypeDict.ContainsKey(arg))
                {
                    stack.Push(Parser.InvertTypeDict[arg]);
                }
                else
                {
                    throw new KulaException.KTypeException(arg.Name);
                }
            } },

            // Bool
            {"equal", (args, stack, engine) => {
                stack.Push( object.Equals(args[0], args[1]) ? 1f : 0f);
            } },
            {"greater", (args, stack, engine) => {
                ArgsCheck(args, typeof(float), typeof(float));
                stack.Push( ((float)args[0] > (float)args[1]) ? 1f : 0f);
            } },
            {"less",  (args, stack, engine) => {
                ArgsCheck(args, typeof(float), typeof(float));
                stack.Push( ((float)args[0] < (float)args[1]) ? 1f : 0f);
            } },
            {"and",  (args, stack, engine) => {
                ArgsCheck(args, typeof(float), typeof(float));
                bool flag = ((float)args[0] != 0) && ((float)args[1] != 0);
                stack.Push(flag ? 1f : 0f);
            } },
            {"or", (args, stack, engine) => {
                ArgsCheck(args, typeof(float), typeof(float));
                bool flag = ((float)args[0] != 0) || ((float)args[1] != 0);
                stack.Push(flag ? 1f : 0f);
            } },
            {"not",  (args, stack, engine) => {
                ArgsCheck(args, typeof(float));
                stack.Push((float)args[0] == 0f ? 1f : 0f);
            } },

            // Array
            {"newArray", (args, stack, engine) => {
                ArgsCheck(args, typeof(float));
                Array tmp = new Array((int)(float)args[0]);
                stack.Push(tmp);
            } },
            {"fill", (args, stack, engine) => {
                ArgsCheck(args, typeof(Array), typeof(float), typeof(object));
                ((Array)args[0]).Data[(int)(float)args[1]] = args[2];
            } },
            {"size", (args, stack, engine) => {
                ArgsCheck(args, typeof(Array));
                stack.Push((float) ((Array)args[0]).Data.Length);
            } },

            // Map
            {"newMap", (args, stack, engine) =>{
                Map tmp_map = new Map();
                stack.Push(tmp_map);
            } },
            {"put", (args, stack, engine) => {
                ArgsCheck(args, typeof(Map), typeof(string), typeof(object));
                ((Map)args[0]).Data[(string)args[1]] = args[2];
            } },
            {"count", (args, stack, engine) => {
                ArgsCheck(args, typeof(Map));
                stack.Push((float) ((Map)args[0]).Data.Count);
            } },
            {"keyIn", (args, stack, engine) => {
                ArgsCheck(args, typeof(Map), typeof(string));
                stack.Push((float) (((Map)args[0]).Data.ContainsKey((string)args[1]) ? 1f : 0f ));
            } },
            {"for", (args, stack, engine) =>
            {
                ArgsCheck(args, typeof(Map), typeof(FuncWithEnv));
                foreach(var kv in ((Map)args[0]).Data)
                {
                    new FuncRuntime((FuncWithEnv)args[1], stack, engine).Run(new object[2] { kv.Key, kv.Value });
                }
            } },

            // ASCII
            {"AsciiToChar", (args, stack, engine) => {
                ArgsCheck(args, typeof(float));
                stack.Push(((char)(int)(float)args[0]).ToString());
            } },
            {"CharToAscii", (args, stack, engine) => {
                ArgsCheck(args, typeof(string));
                if (!float.TryParse((string)args[0], out float tmp)) { tmp = 0; }
                stack.Push(tmp);
            } },


            // Exception
            {"throw", (args, stack, engine) => {
                ArgsCheck(args, typeof(string));
                throw new KulaException.UserException((string)args[0]);
            } },
        };

        /// <summary>
        /// 类型断言
        /// </summary>
        /// <param name="args">参数数组</param>
        /// <param name="types">类型数组</param>
        private static void ArgsCheck(object[] args, params Type[] types)
        {
            bool flag = args.Length == types.Length;
            for (int i = 0; i < args.Length && flag; i++)
            {
                flag = types[i] == typeof(object) || args[i].GetType() == types[i];
                if (!flag) throw new KulaException.ArgsTypeException(args[i].GetType().Name, types[i].Name);
            }
        }

        // 接口儿
        public List<LexToken> TokenStream { get; }
        public List<VMNode> NodeStream { get; }
        public List<Type> ArgTypes { get; }
        public List<string> ArgNames { get; }
        public Type ReturnType { get; set; }

        public Func(List<LexToken> tokenStream)
        {
            this.TokenStream = tokenStream;

            this.ArgTypes = new List<Type>();
            this.ArgNames = new List<string>();
            this.NodeStream = new List<VMNode>();
        }

        private string @string = null;

        public override string ToString()
        {
            if (@string == null)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("func(");
                for (int i = 0; i < ArgTypes.Count; ++i)
                {
                    if (i != 0)
                    {
                        sb.Append(',');
                    }
                    sb.Append(ArgTypes[i].KTypeToString());
                }
                sb.Append("):");
                sb.Append(ReturnType.KTypeToString());
                @string = sb.ToString();
            }
            return @string;
        }
    }
}
