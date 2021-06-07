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
            mainRuntime = new FuncRuntime(null, null);
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
                Console.ForegroundColor = ConsoleColor.White;
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
                Console.WriteLine(e);
                Console.ForegroundColor = ConsoleColor.White;
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
        public static Queue<object> KulaQueue { get => kulaQueue; }
        private static readonly Queue<object> kulaQueue = new Queue<object>();

        /**
        private static void Main(string[] args)
        {
            if (args.Length >= 1)
            {
                string code;
                try
                {
                    code = File.ReadAllText(args[0]);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return;
                }

                if (args.Length >= 2 && args[1] == "--release")
                {
                    ConsoleUtility.ReleaseRunCode(code);
                }
                else
                {
                    ConsoleUtility.DebugRunCode(code);
                }
            }
            else
            {
                KulaVersion.HelloKula();
                ConsoleUtility.DebugREPL();
            }
        }
        **/
    }
}
