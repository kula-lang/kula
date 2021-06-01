using System;
using System.Collections.Generic;

using kula.Data;
using kula.Util;

namespace kula.Core
{
    class FuncRuntime   // : IRuntime
    {
        private static readonly FuncRuntime mainRuntime = new FuncRuntime(null, null);
        public static FuncRuntime MainRuntime { get => mainRuntime; }

        private readonly Dictionary<string, Object> varDict;
        private Func root;

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
        public Func Root => root;

        public FuncRuntime Read(Func funcEnv)
        {
            root = funcEnv;
            return this;
        }
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
                                    /*
                                    bool flag = false;
                                    Queue<FuncRuntime> fr_que = new Queue<FuncRuntime>();
                                    HashSet<FuncRuntime> fr_set = new HashSet<FuncRuntime>();
                                    if (fr_set.Add(this))
                                        fr_que.Enqueue(this);
                                    if (fr_set.Add(root.Runtime))
                                        fr_que.Enqueue(root.Runtime);
                                    while (flag == false && fr_que.Count > 0)
                                    {
                                        FuncRuntime now_env = fr_que.Dequeue(); 
                                        if (now_env != null)
                                        {
                                            if (fr_set.Add(now_env.Root.Runtime))
                                                fr_que.Enqueue(now_env.Root.Runtime);
                                            if (fr_set.Add(now_env.Root.Func.FatherRuntime))
                                                fr_que.Enqueue(now_env.Root.Func.FatherRuntime); 

                                            if (now_env.VarDict.ContainsKey((string)node.Value))
                                            {
                                                now_env.VarDict[(string)node.Value] = envStack.Pop();
                                                flag = true;
                                            }
                                        }
                                    }*/
                                    bool flag = false;
                                    FuncRuntime now_env = this;
                                    while (flag == false && now_env != null)
                                    {
                                        if (now_env.VarDict.ContainsKey((string)node.Value))
                                        {
                                            now_env.VarDict[(string)node.Value] = envStack.Pop();
                                            flag = true;
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
                                    /*
                                    bool flag = false;
                                    Queue<FuncRuntime> fr_que = new Queue<FuncRuntime>();
                                    HashSet<FuncRuntime> fr_set = new HashSet<FuncRuntime>();
                                    if (fr_set.Add(this))
                                        fr_que.Enqueue(this);
                                    if (fr_set.Add(root.Runtime))
                                        fr_que.Enqueue(root.Runtime);

                                    while (false == flag && fr_que.Count > 0)
                                    {
                                        FuncRuntime now_env = fr_que.Dequeue();
                                        if (now_env != null)
                                        {
                                            if (fr_set.Add(now_env.Root.Runtime))
                                                fr_que.Enqueue(now_env.Root.Runtime);
                                            if (fr_set.Add(now_env.Root.Func.FatherRuntime))
                                                fr_que.Enqueue(now_env.Root.Func.FatherRuntime);

                                            if (now_env.VarDict.ContainsKey((string)node.Value))
                                            {
                                                object node_value = now_env.VarDict[(string)node.Value];
                                                envStack.Push(node_value);
                                                flag = true;
                                            }
                                        }
                                    }*/
                                    bool flag = false;
                                    FuncRuntime now_env = this;
                                    while (flag == false && now_env != null)
                                    {
                                        if (now_env.VarDict.ContainsKey((string)node.Value))
                                        {
                                            envStack.Push(now_env.VarDict[(string)node.Value]);
                                            flag = true;
                                        }
                                        now_env = now_env.Root.FatherRuntime;
                                    }
                                    if (!flag) 
                                    { 
                                        throw new KulaException.VariableException(); 
                                    }
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
                                        object func_env = varDict[func_name];
                                        if (func_env is Func)
                                        {
                                            new FuncRuntime((Func)func_env, envStack).Run();
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
