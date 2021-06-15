using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

using Kula.Core;
using Kula.Data;

namespace Kula
{
    public class KulaEngine
    {
        private readonly FuncRuntime mainRuntime;
        private readonly Dictionary<string, FuncEnv> byteCodeMap = new Dictionary<string, FuncEnv>();
        private readonly Queue<object> queue = new Queue<object>();
        public Queue<object> EngineQueue { get => queue; }

        public KulaEngine()
        {
            mainRuntime = new FuncRuntime(null, null, queue);
        }

        public void Compile(string sourceCode, string codeID, bool isDebug)
        {
            var tmp1 = Lexer.Instance.Read(sourceCode).Scan();
            if (isDebug) { tmp1.Show(); }
            List<LexToken> lexTokens = tmp1.Out();

            Func main = new Func(lexTokens) { Compiled = true };
            FuncEnv mainEnv = new FuncEnv(main, null);
            
            var tmp2 = Parser.Instance.Parse(main);
            if (isDebug) { tmp2.Show(); }
            
            byteCodeMap[codeID] = mainEnv;
        }

        public void Run(string codeId, bool isDebug)
        {
            if (!isDebug)
            {
                mainRuntime.Read(byteCodeMap[codeId]).Run(null);
            }
            else
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                mainRuntime.Read(byteCodeMap[codeId]).Run(null);
                stopwatch.Stop();
                Console.WriteLine("\tIt takes " + stopwatch.Elapsed.Milliseconds + " ms.\n");
            }
        }

        public void Clear()
        {
            mainRuntime.VarDict.Clear();
            mainRuntime.EnvStack.Clear();
        }

        public static void Hello()
        {
            Kula.Util.KulaVersion.HelloKula();
        }

        // 静态
        private static readonly Dictionary<string, BuiltinFunc> extendFunc = new Dictionary<string, BuiltinFunc>();
        public static Dictionary<string, BuiltinFunc> ExtendFunc { get => extendFunc; }
    }
}
