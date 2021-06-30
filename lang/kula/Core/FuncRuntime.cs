using System;
using System.Collections.Generic;

using Kula.Data;
using Kula.Util;

namespace Kula.Core
{
    class FuncRuntime   // : IRuntime
    {
        private readonly Dictionary<string, object> varDict;
        private FuncEnv root;
        private bool returned;

        private readonly KulaEngine engine;
        private readonly Stack<object> envStack;
        private readonly Stack<object> fatherStack;

        public FuncRuntime(FuncEnv root, Stack<object> fatherStack, KulaEngine engine)
        {
            this.root = root;
            this.fatherStack = fatherStack;
            this.engine = engine;

            this.envStack = new Stack<object>();
            this.varDict = new Dictionary<string, object>();
        }

        public void Clear()
        {
            this.varDict.Clear();
            this.envStack.Clear();
        }

        public FuncRuntime Read(FuncEnv func)
        {
            root = func;
            return this;
        }

        /// <summary>
        /// 函数调用
        /// Release 模式运行
        /// </summary>
        /// <param name="arguments">函数调用的参数列表</param>
        public void Run(object[] arguments)
        {
            if ((arguments == null ? 0 : arguments.Length) != root.Func.ArgNames.Count)
            {
                throw new KulaException.FuncArgumentException();
            }
            for (int i = root.Func.ArgNames.Count - 1; i >= 0 && arguments != null; --i)
            {
                object arg = arguments[i];
                if (root.Func.ArgTypes[i] != typeof(object) && arg.GetType() != root.Func.ArgTypes[i])
                {
                    throw new KulaException.ArgsTypeException(root.Func.ArgTypes[i].Name, arg.GetType().Name);
                }
                else
                {
                    varDict.Add(root.Func.ArgNames[i], arg);
                }
            }

            envStack.Clear();

            returned = false;
            for (int i = 0; i < root.Func.NodeStream.Count && returned == false; ++i)
            {
                var node = root.Func.NodeStream[i];
                try
                {
                    switch (node.Type)
                    {
                        case VMNodeType.VALUE:
                            {
                                envStack.Push(node.Value);
                            }
                            break;
                        case VMNodeType.STRING:
                            {
                                string val = (string)node.Value;
                                val = System.Text.RegularExpressions.Regex.Unescape(val);
                                envStack.Push(val);
                            }
                            break;
                        case VMNodeType.LAMBDA:
                            {
                                object value = node.Value;
                                envStack.Push(new FuncEnv((Func)value, this));
                            }
                            break;
                        case VMNodeType.LET:
                            {
                                bool flag = false;
                                FuncRuntime now_env = this;

                                while (flag == false && now_env != null)
                                {
                                    if (now_env.varDict.ContainsKey((string)node.Value))
                                    {
                                        now_env.varDict[(string)node.Value] = envStack.Pop();
                                        flag = true;
                                    }
                                    now_env = now_env.root.Runtime;
                                }
                                if (!flag)
                                {
                                    varDict[(string)node.Value] = envStack.Pop();
                                }
                            }
                            break;
                        case VMNodeType.VAR:
                            {
                                this.varDict[(string)node.Value] = envStack.Pop();
                            }
                            break;
                        case VMNodeType.NAME:
                            {
                                bool flag = false;
                                FuncRuntime now_env = this;

                                // 扩展函数 覆盖 内置函数
                                if (KulaEngine.ExtendFunc.ContainsKey((string)node.Value))
                                {
                                    flag = true;
                                    envStack.Push(KulaEngine.ExtendFunc[(string)node.Value]);
                                }
                                else if (Func.BuiltinFunc.ContainsKey((string)node.Value))
                                {
                                    flag = true;
                                    envStack.Push(Func.BuiltinFunc[(string)node.Value]);
                                }

                                while (flag == false && now_env != null)
                                {
                                    if (now_env.varDict.ContainsKey((string)node.Value))
                                    {
                                        envStack.Push(now_env.varDict[(string)node.Value]);
                                        flag = true;
                                    }
                                    now_env = now_env.root.Runtime;
                                }
                                if (!flag)
                                {
                                    throw new KulaException.VariableException(node.Value.ToString());
                                }
                            }
                            break;
                        case VMNodeType.FUNC:
                            {
                                // 按 参数个数 获取 参数值
                                object[] args = new object[(int)node.Value];
                                for (int k = args.Length - 1; k >= 0; --k)
                                {
                                    args[k] = envStack.Pop();
                                }
                                object func = envStack.Pop();
                                if (func is BuiltinFunc builtin_func)
                                {
                                    builtin_func(args, envStack);
                                }
                                else if (func is FuncEnv func_env)
                                {
                                    new FuncRuntime(func_env, envStack, engine).Run(args);
                                }
                                else
                                {
                                    throw new KulaException.FuncUsingException(func.ToString());
                                }
                            }
                            break;
                        case VMNodeType.IFGOTO:
                            {
                                float arg = (float)envStack.Pop();
                                if (arg == 0)
                                {
                                    i = (int)node.Value - 1;
                                }
                            }
                            break;
                        case VMNodeType.GOTO:
                            {
                                i = (int)node.Value - 1;
                            }
                            break;
                        case VMNodeType.RETURN:
                            {
                                object return_val = envStack.Pop();
                                if (root.Func.ReturnType != typeof(object) && return_val.GetType() != root.Func.ReturnType)
                                {
                                    throw new KulaException.ReturnValueException(return_val.GetType().Name, root.Func.ReturnType.Name);
                                }
                                fatherStack.Push(return_val);
                                returned = true;
                            }
                            break;
                        case VMNodeType.CONKEY:
                            {
                                object vector_key = envStack.Pop();
                                object vector = envStack.Pop();
                                if ((char)node.Value == '[')
                                {
                                    if (vector_key.GetType() != typeof(float) || vector.GetType() != typeof(Data.Array))
                                    {
                                        throw new KulaException.ArrayTypeException();
                                    }
                                    envStack.Push(
                                        ((Data.Array)vector).Data
                                            [(int)(float)vector_key]
                                    );
                                }
                                else if ((char)node.Value == '<')
                                {
                                    if (vector_key.GetType() != typeof(string) || vector.GetType() != typeof(Data.Map))
                                    {
                                        throw new KulaException.MapTypeException();
                                    }
                                    envStack.Push(
                                        ((Data.Map)vector).Data[(string)vector_key]
                                    );
                                }
                            }
                            break;
                    }
                }
                catch (InvalidOperationException)
                {
                    throw new KulaException.VMUnderflowException();
                }
            }
            if (returned == false && root.Func.ReturnType != null)
            {
                throw new KulaException.ReturnValueException("None", root.Func.ReturnType.Name);
            }
        }

        /// <summary>
        /// Debug 模式运行
        /// </summary>
        public void DebugRun()
        {
            Console.WriteLine("Output ->");
            Run(null);
            Show();
        }

        /// <summary>
        /// Debug 模式下，输出实时编译信息
        /// </summary>
        public void Show()
        {
            int cnt = 0;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("\nVM ->");
            Console.ForegroundColor = ConsoleColor.White;
            while (envStack.Count > 0)
            {
                object tmp = envStack.Pop();
                if (tmp.GetType() == typeof(string))
                {
                    tmp = "\"" + tmp + "\"";
                }
                Console.WriteLine("\tVM-Stack {" + cnt++ + "} : " + tmp);
            }
            Console.WriteLine("\tEnd Of Kula Program\n");
            Console.ResetColor();
        }
    }
}
