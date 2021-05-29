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
        private readonly IRuntime father;

        private readonly Stack<object> envStack;

        public FuncRuntime(Func root, IRuntime father)
        {
            this.root = root;
            this.father = father;
            this.envStack = new Stack<object>();
            this.varDict = new Dictionary<string, object>();
        }

        public IRuntime Father => father;
        public Dictionary<string, object> VarDict => varDict;
        public Stack<object> EnvStack => envStack;
        public void Run()
        {
            if (root.NodeStream.Count == 0)
            {
                Parser.Instance.ParseLambda(root, father);
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
                    object arg = Father.EnvStack.Pop();
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
                                    envStack.Push(node.Value);
                                }
                                break;
                            case KvmNodeType.VARIABLE:
                                {
                                    // varDict[(string)node.Value] = envStack.Pop();
                                    IRuntime now_env = this;
                                    bool flag = false;
                                    while (now_env.Father != null)
                                    {
                                        if (now_env.VarDict.ContainsKey((string)node.Value))
                                        {
                                            VarDict[(string)node.Value] = envStack.Pop();
                                            flag = true;
                                            break;
                                        }
                                        now_env = now_env.Father;
                                    }
                                    if (!flag) { varDict[(string)node.Value] = envStack.Pop(); }
                                }
                                break;
                            case KvmNodeType.NAME:
                                {
                                    // envStack.Push(VarDict[(string)node.Value]);
                                    IRuntime now_env = this;
                                    bool flag = false;
                                    while (now_env.Father != null)
                                    {
                                        if (now_env.VarDict.ContainsKey((string)node.Value))
                                        {
                                            object node_value = now_env.VarDict[(string)node.Value];
                                            envStack.Push(node_value);
                                            flag = true;
                                            break;
                                        }
                                        now_env = now_env.Father;
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
                                    Father.EnvStack.Push(envStack.Pop());
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
