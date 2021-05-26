using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kula.Core.VMObj;
using kula.DataObj;

namespace kula.Core
{
    class KulaVM : IRuntime
    {
        private static KulaVM instance = new KulaVM();
        public static KulaVM Instance { get => instance; }

        public IRuntime Father => null;
        public Dictionary<string, object> VarDict => varDict;

        private Dictionary<string, object> varDict = new Dictionary<string, object>();
        private List<KvmNode> nodeStream;
        private Stack<object> vmStack;

        private KulaVM()
        {
            vmStack = new Stack<object>();
        }
        public KulaVM Read(List<KvmNode> nodeStream)
        {
            this.nodeStream = nodeStream;
            return this;
        }
        public void Run()
        {
            vmStack.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Output ->");
            for (int i=0; i<nodeStream.Count; ++i)
            {
                var node = nodeStream[i];
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
                            /*
                            if (VarDict.ContainsKey((string)node.Value))
                            {
                                varDict[(string)node.Value] = vmStack.Pop();
                            }
                            else
                            {
                                throw new Exception("运行时错误：没有变量");
                            }*/
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
                                throw new Exception("没做呢");
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
            Console.WriteLine();
        }
        public void DebugRun()
        {
            Run(); Show();
        }

        public void Show()
        {
            int cnt = 0;
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
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}
