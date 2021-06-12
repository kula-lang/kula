using System;
using System.Collections.Generic;

using Kula.Util;
using Kula.Core;

namespace Kula.Data
{
    public delegate void BuiltinFunc(object[] args, Stack<object> stack);
    class Func  // : IRunnable
    {
        // 静态内置方法 们
        public static Dictionary<string, BuiltinFunc> BuiltinFunc { get => builtinFunc; }
        private static readonly Dictionary<string, BuiltinFunc> builtinFunc = new Dictionary<string, BuiltinFunc>()
        {
            // Num
            {"plus", (args, stack) => {
                ArgsCheck(args, new Type[]{typeof(float), typeof(float)});
                stack.Push((float)args[0] + (float)args[1]);
            } },
            {"minus", (args, stack) => {
                ArgsCheck(args, new Type[]{typeof(float), typeof(float)});
                stack.Push((float)args[0] - (float)args[1]);
            } },
            {"times", (args, stack) => {
                ArgsCheck(args, new Type[]{typeof(float), typeof(float)});
                stack.Push((float)args[0] * (float)args[1]);
            } },
            {"div", (args, stack) => {
                ArgsCheck(args, new Type[]{typeof(float), typeof(float)});
                stack.Push((float)args[0] / (float)args[1]);
            } },

            // IO
            {"print", (args, stack) => {
                foreach (var arg in args)
                {
                    Console.Write( KToString(arg) );
                }
            } },
            {"println", (args, stack) => {
                foreach (var arg in args)
                {
                    Console.Write( KToString(arg) );
                }
                Console.WriteLine();
            } },
            {"input", (args, stack) =>{
                stack.Push(Console.ReadLine());
            } },
            
            // String
            {"toStr", (args, stack) => {
                stack.Push(KToString(args[0]));
            } },
            {"parseNum", (args, stack) => {
                var arg = args[0];
                ArgsCheck(args, new Type[] { typeof(string) });
                float.TryParse((string)arg, out float ans);
                stack.Push(ans);
            } },
            {"len", (args, stack) => {
                ArgsCheck(args, new Type[] { typeof(string) });
                stack.Push((float)((string)args[0]).Length);
            } },
            {"cut", (args, stack) => {
                ArgsCheck(args, new Type[] { typeof(string), typeof(float), typeof(float) });
                stack.Push(((string)args[0]).Substring((int)(float)args[1], (int)(float)args[2]));
            } },
            {"concat", (args, stack) => {
                ArgsCheck(args, new Type[] { typeof(string), typeof(string) });
                stack.Push((string)args[0] + (string)args[1]);
            } },
            {"type", (args, stack) => {
                var arg_type = args[0].GetType();
                switch (Type.GetTypeCode(arg_type))
                {
                    case TypeCode.Single:
                        stack.Push("Num");
                        break;
                    case TypeCode.String:
                        stack.Push("Str");
                        break;
                    default:
                        if(arg_type == typeof(FuncEnv))
                            stack.Push("Func");
                        else if (arg_type == typeof(Array))
                            stack.Push("Array");
                        else if (arg_type == typeof(Map))
                            stack.Push("Map");
                        else
                            stack.Push("None");
                        break;
                }
            } },

            // Bool
            {"equal", (args, stack) => {
                stack.Push( object.Equals(args[0], args[1]) ? 1f : 0f);
            } },
            {"greater", (args, stack) => {
                ArgsCheck(args, new Type[] { typeof(float), typeof(float) });
                stack.Push( ((float)args[0] > (float)args[1]) ? 1f : 0f);
            } },
            {"less",  (args, stack) => {
                ArgsCheck(args, new Type[] { typeof(float), typeof(float) });
                stack.Push( ((float)args[0] < (float)args[1]) ? 1f : 0f);
            } },
            {"and",  (args, stack) => {
                ArgsCheck(args, new Type[] { typeof(float), typeof(float) });
                bool flag = ((float)args[0] != 0) && ((float)args[1] != 0);
                stack.Push(flag ? 1f : 0f);
            } },
            {"or", (args, stack) => {
                ArgsCheck(args, new Type[] { typeof(float), typeof(float) });
                bool flag = ((float)args[0] != 0) || ((float)args[1] != 0);
                stack.Push(flag ? 1f : 0f);
            } },
            {"not",  (args, stack) => {
                ArgsCheck(args, new Type[] { typeof(float) });
                stack.Push((float)args[0] == 0f ? 1f : 0f);
            } },

            // Array
            {"newArray", (args, stack) => {
                ArgsCheck(args, new Type[] { typeof(float) });
                Array tmp = new Array((int)(float)args[0]);
                stack.Push(tmp);
            } },
            {"fill", (args, stack) => {
                ArgsCheck(args, new Type[] { typeof(Array), typeof(float), typeof(object) });
                ((Array)args[0])[(int)(float)args[1]] = args[2];
            } },

            // Map
            {"newMap", (args, stack) =>{
                Map tmp_map = new Map();
                stack.Push(tmp_map);
            } },
            {"let", (args, stack) => {
                ArgsCheck(args, new Type[] { typeof(Map), typeof(string), typeof(object) });
                ((Map)args[0]).Data[(string)args[1]] = args[2];
            } },
            {"count", (args, stack) => {
                ArgsCheck(args, new Type[] { typeof(Map) });
                stack.Push((float) ((Map)args[0]).Data.Count);
            } },
            {"keyIn", (args, stack) => {
                ArgsCheck(args, new Type[] { typeof(Map), typeof(string) });
                stack.Push((float) (((Map)args[0]).Data.ContainsKey((string)args[1]) ? 1f : 0f ));
            } },

            // ASCII
            {"AsciiToChar", (args, stack) => {
                ArgsCheck(args, new Type[] { typeof(float) });
                stack.Push(((char)(int)(float)args[0]).ToString());
            } },
            {"CharToAscii", (args, stack) => {
                ArgsCheck(args, new Type[] { typeof(string) });
                if (!float.TryParse((string)args[0], out float tmp)) { tmp = 0; }
                stack.Push(tmp);
            } },

            // Sharp API
            {"_enqueue", (args, stack) => {
                foreach(var arg in args)
                {
                    KulaEngine.KulaQueue.Enqueue(arg);
                }
            } },
            {"_dequeue", (args, stack) => {
                ArgsCheck(args, new Type[0]);
                stack.Push(KulaEngine.KulaQueue.Dequeue());
            } },
            {"_peek", (args, stack) => {
                ArgsCheck(args, new Type[0]);
                stack.Push(KulaEngine.KulaQueue.Peek());
            } },
            {"_count", (args, stack) => {
                ArgsCheck(args, new Type[0]);
                stack.Push((float)KulaEngine.KulaQueue.Count);
            } },
            {"_clear", (args, stack) => {
                ArgsCheck(args, new Type[0]);
                KulaEngine.KulaQueue.Clear();
            } },

            // Exception
            {"throw", (args, stack) => {
                ArgsCheck(args, new Type[] {typeof(string) });
                throw new KulaException.UserException((string)args[0]);
            } },
        };

        private static void ArgsCheck(object[] args, Type[] types)
        {
            bool flag = args.Length == types.Length;
            for (int i = 0; i < args.Length && flag; i++)
            {
                flag = types[i] == typeof(object) || args[i].GetType() == types[i];
            }
            if (flag == false) throw new KulaException.FuncTypeException();
        }
        private static string KToString(object arg)
        {
            if (arg.GetType() == typeof(BuiltinFunc))
            {
                return "<BuiltinFunc/>";
            }
            else
            {
                return arg.ToString();
            }
        }

        // 接口儿
        public List<LexToken> TokenStream { get => tokenStream; }
        public List<VMNode> NodeStream { get => nodeStream; }
        public bool Compiled { get => compiled; set => compiled = true; }
        public List<Type> ArgTypes { get => argTypes; }
        public List<string> ArgNames { get => argNames; }
        public Type ReturnType { get => returnType; set => returnType = value; }


        private bool compiled;
        private readonly List<LexToken> tokenStream;
        private readonly List<VMNode> nodeStream;

        private readonly List<Type> argTypes;
        private readonly List<string> argNames;
        private Type returnType;

        public Func(List<LexToken> tokenStream)
        {
            this.tokenStream = tokenStream;

            this.argTypes = new List<Type>();
            this.argNames = new List<string>();
            this.nodeStream = new List<VMNode>();
        }

        public override string ToString()
        {
            return "<o_O> lambda";
        }
    }
}
