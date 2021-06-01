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
        private FuncEnv root;
        private bool returned;

        private readonly Stack<object> envStack;
        private readonly Stack<object> fatherStack;

        public FuncRuntime(FuncEnv root, Stack<object> fatherStack)
        {
            this.root = root;
            this.fatherStack = fatherStack;
            this.envStack = new Stack<object>();
            this.varDict = new Dictionary<string, object>();
        }

        public Dictionary<string, object> VarDict => varDict;
        public Stack<object> EnvStack => envStack;
        public Stack<object> FatherStack => fatherStack;

        public FuncRuntime Read(FuncEnv func)
        {
            root = func;
            return this;
        }
        public void Run()
        {
            if (root.Runtime != null && root.Func.NodeStream.Count == 0)
            {
                Parser.Instance.ParseLambda(root.Func);
            }
            
            if (root.Func.TokenStream.Count == 0 && root.Func.NodeStream.Count != 0)
            {
                for (int i = root.Func.ArgNames.Count - 1; i >= 0; --i)
                {
                    object arg = FatherStack.Pop();
                    if (arg.GetType() != root.Func.ArgTypes[i])
                    {
                        throw new KulaException.FuncException();
                    }
                    else
                    {
                        varDict.Add(root.Func.ArgNames[i], arg);
                    }
                }

                envStack.Clear();
                Console.ForegroundColor = ConsoleColor.White;

                returned = false;
                for (int i = 0; i < root.Func.NodeStream.Count && returned == false; ++i)
                {
                    var node = root.Func.NodeStream[i];
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
                            case KvmNodeType.LAMBDA:
                                {
                                    object value = node.Value;
                                    envStack.Push(new FuncEnv((Func)value, this));
                                }
                                break;
                            case KvmNodeType.VARIABLE:
                                {
                                    /**
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
                                        }
                                    **/
                                    bool flag = false;
                                    FuncRuntime now_env = this;
                                    while (flag == false && now_env != null)
                                    {
                                        if (now_env.VarDict.ContainsKey((string)node.Value))
                                        {
                                            now_env.VarDict[(string)node.Value] = envStack.Pop();
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
                            case KvmNodeType.NAME:
                                {
                                    /**
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
                                    }
                                    **/
                                    bool flag = false;
                                    FuncRuntime now_env = this;
                                    while (flag == false && now_env != null)
                                    {
                                        if (now_env.VarDict.ContainsKey((string)node.Value))
                                        {
                                            envStack.Push(now_env.VarDict[(string)node.Value]);
                                            flag = true;
                                        }
                                        now_env = now_env.root.Runtime;
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
                                        FuncRuntime now_env = this;
                                        object this_func = null;
                                        while (this_func == null && now_env != null)
                                        {
                                            if (now_env.VarDict.ContainsKey((string)node.Value))
                                            {
                                                this_func = now_env.VarDict[(string)node.Value];
                                            }
                                            now_env = now_env.root.Runtime;
                                        }
                                        if (this_func == null)
                                        {
                                            throw new KulaException.VariableException();
                                        }
                                        if (this_func is FuncEnv)
                                        {
                                            new FuncRuntime((FuncEnv)this_func, envStack).Run();
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
                                    object return_val = envStack.Pop();
                                    if (root.Func.ReturnType != typeof(object) && return_val.GetType() != root.Func.ReturnType)
                                    {
                                        throw new KulaException.ReturnValueException();
                                    }
                                    FatherStack.Push(return_val);
                                    returned = true;
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
                }
                if (returned == false && root.Func.ReturnType != null)
                {
                    throw new KulaException.ReturnValueException();
                }
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
