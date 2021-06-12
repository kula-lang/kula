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
        public KulaEngine()
        {
            mainRuntime = new FuncRuntime(null, null, this.queue);
        }
        public void Run(string sourceCode)
        {
            try
            {
                List<LexToken> lexTokens = Lexer.Instance.Read(sourceCode).Scan().Out();
                Func main = new Func(lexTokens) { Compiled = true };
                FuncEnv mainEnv = new FuncEnv(main, null);
                Parser.Instance.Parse(main);
                mainRuntime.Read(mainEnv).Run(null);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ResetColor();
            }
        }
        public void DebugRun(string code)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                List<LexToken> lexTokens = Lexer.Instance.Read(code).Scan().Show().Out();
                Func main = new Func(lexTokens) { Compiled = true };
                FuncEnv mainEnv = new FuncEnv(main, null);
                Parser.Instance.Parse(main).Show();
                mainRuntime.Read(mainEnv).DebugRun();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ResetColor();
            }
            stopwatch.Stop();
            Console.WriteLine("\tIt takes " + stopwatch.Elapsed.Milliseconds + " ms.\n");
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
        public Queue<object> EngineQueue { get => queue; }
        public Dictionary<string, BuiltinFunc> ExtendFunc { get => extendFunc; }

        private readonly Queue<object> queue = new Queue<object>();
        private readonly Dictionary<string, BuiltinFunc> extendFunc = new Dictionary<string, BuiltinFunc>();
    }
}
