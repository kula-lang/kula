using System;
using System.Collections.Generic;

using kula.Data;
using kula.Util;

namespace kula.Core
{
    class KulaVM : IRuntime
    {
        // 单例模式
        private static KulaVM instance = new KulaVM();
        public static KulaVM Instance { get => instance; }

        // 开放接口
        public IRuntime Father => null;
        public Stack<object> EnvStack => vmStack;
        public Dictionary<string, object> VarDict => varDict;

        // 私有成员
        private Dictionary<string, object> varDict = new Dictionary<string, object>();
        private IRunnable root;
        private readonly Stack<object> vmStack;

        private KulaVM()
        {
            vmStack = new Stack<object>();
        }
        public KulaVM Read(IRunnable root)
        {
            this.root = root;
            return this;
        }
        public void Run()
        {
            vmStack.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            
            for (int i=0; i<root.NodeStream.Count; ++i)
            {
                var node = root.NodeStream[i];
                try
                {
                    switch (node.Type)
                    {
                        case KvmNodeType.VALUE:
                        case KvmNodeType.STRING:
                            {
                                vmStack.Push(node.Value);
                            }
                            break;
                        case KvmNodeType.VARIABLE:
                            {
                                varDict[(string)node.Value] = vmStack.Pop();
                            }
                            break;
                        case KvmNodeType.NAME:
                            {
                                vmStack.Push(varDict[(string)node.Value]);
                            }
                            break;
                        case KvmNodeType.FUNC:
                            {
                                string func_name = (string)node.Value;
                                if (Func.BuiltinFunc.ContainsKey(func_name))
                                {
                                    Func.BuiltinFunc[func_name](vmStack);
                                }
                                else
                                {
                                    object func = varDict[func_name];
                                    if (func.GetType() == typeof(Func))
                                    {
                                        new FuncRuntime((Func)func, this).Run();
                                    }
                                    else
                                    {
                                        throw new KulaException.FuncException();
                                    }
                                }
                            }
                            break;
                        case KvmNodeType.IFGOTO:
                            {
                                float arg = (float)vmStack.Pop();
                                if (arg == 0)
                                {
                                    i = (int)node.Value - 1;
                                }
                            }
                            break;
                        case KvmNodeType.GOTO:
                            {
                                i = (int)node.Value - 1;
                            }
                            break;
                    }
                }
                catch (InvalidOperationException)
                {
                    throw new KulaException.VMUnderflowException();
                }
                catch (OutOfMemoryException)
                {
                    throw new KulaException.VMOverflowException();
                }
                catch (KeyNotFoundException)
                {
                    throw new KulaException.UnknownNameException();
                }
                
            }
            Console.WriteLine();
        }
        public void DebugRun()
        {
            Console.WriteLine("Output ->");
            Run(); 
            Show();
        }

        public void Show()
        {
            int cnt = 0;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("VM ->");
            Console.ForegroundColor = ConsoleColor.White;
            while (vmStack.Count > 0)
            {
                object tmp = vmStack.Pop();
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
