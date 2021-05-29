using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using kula.Data;
using kula.Util;

namespace kula.Core
{
    class FuncRuntime : IRuntime
    {
        private Dictionary<string, Object> varDict;
        private readonly Func root;

        private readonly Stack<object> envStack;
        private readonly Stack<object> fatherStack;

        public FuncRuntime(Func root, Stack<object> fatherStack)
        {
            this.root = root;
            this.fatherStack = fatherStack;
            this.envStack = new Stack<object>();
            this.varDict = new Dictionary<string, object>();
        }

        public Dictionary<string, object> VarDict => varDict;
        public Stack<object> EnvStack => envStack;
        public Stack<object> FatherStack => fatherStack;
        public IRunnable Root => root;

        public void Run()
        {
            if (root.NodeStream.Count == 0)
            {
                Parser.Instance.ParseLambda(root);
            }
            
            if (root.TokenStream.Count == 0 && root.NodeStream.Count != 0)
            {
                /*
                foreach (var node in root.NodeStream)
                {
                    Console.WriteLine(node);
                }
                */
                for (int i = root.ArgNames.Count - 1; i >= 0; --i)
                {
                    object arg = FatherStack.Pop();
                    if (arg.GetType() != root.ArgTypes[i])
                    {
                        throw new KulaException.FuncException();
                    }
                    else
                    {
                        varDict.Add(root.ArgNames[i], arg);
                    }
                }

                envStack.Clear();
                Console.ForegroundColor = ConsoleColor.White;

                for (int i = 0; i < root.NodeStream.Count; ++i)
                {
                    var node = root.NodeStream[i];
                    try
                    {
                        switch (node.Type)
                        {
                            case KvmNodeType.VALUE:
                            case KvmNodeType.STRING:
                                {
                                    object value = node.Value;
                                    if (value is Func)
                                    {
                                        ((Func)value).FatherRuntime = this;
                                    }
                                    envStack.Push(value);
                                }
                                break;
                            case KvmNodeType.VARIABLE:
                                {
                                    IRuntime now_env = this;
                                    bool flag = false;
                                    while (now_env != null)
                                    {
                                        if (now_env.VarDict.ContainsKey((string)node.Value))
                                        {
                                            now_env.VarDict[(string)node.Value] = envStack.Pop();
                                            flag = true;
                                            break;
                                        }
                                        now_env = now_env.Root.FatherRuntime;
                                    }
                                    if (!flag) 
                                    {
                                        varDict[(string)node.Value] = envStack.Pop(); 
                                    }
                                }
                                break;
                            case KvmNodeType.NAME:
                                {
                                    IRuntime now_env = this;
                                    bool flag = false;
                                    while (now_env != null)
                                    {
                                        if (now_env.VarDict.ContainsKey((string)node.Value))
                                        {
                                            object node_value = now_env.VarDict[(string)node.Value];
                                            envStack.Push(node_value);
                                            flag = true;
                                            break;
                                        }
                                        now_env = now_env.Root.FatherRuntime;
                                    }
                                    if (!flag) { throw new Exception("没找着变量，这可咋整"); }
                                }
                                break;
                            case KvmNodeType.FUNC:
                                {
                                    string func_name = (string)node.Value;
                                    if (Func.BuiltinFunc.ContainsKey(func_name))
                                    {
                                        Func.BuiltinFunc[func_name](envStack);
                                    }
                                    else
                                    {
                                        object func = varDict[func_name];
                                        if (func.GetType() == typeof(Func))
                                        {
                                            new FuncRuntime((Func)func, envStack).Run();
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
                                    float arg = (float)envStack.Pop();
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
                            case KvmNodeType.RETURN:
                                {
                                    FatherStack.Push(envStack.Pop());
                                    i = root.NodeStream.Count;
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
            }
            else
            {
                throw new Exception("凉了");
            }
        }
    }
}
