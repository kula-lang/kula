using System;
using System.Collections.Generic;
using System.Diagnostics;

using kula.Core;
using kula.Data;

namespace kula.Util
{
    static class ConsoleUtility
    {
        
        public static void DebugRunCode(string code)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                List<LexToken> lexTokens = Lexer.Instance.Read(code).Scan().Show().Out();
                Func main = new Func(lexTokens) { Compiled = true };
                FuncEnv mainEnv = new FuncEnv(main, null);
                Parser.Instance.Parse(main).Show();
                FuncRuntime.MainRuntime.Read(mainEnv).DebugRun();
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
        public static void ReleaseRunCode(string code)
        {
            try
            {
                List<LexToken> lexTokens = Lexer.Instance.Read(code).Scan().Out();
                Func main = new Func(lexTokens) { Compiled = true };
                FuncEnv mainEnv = new FuncEnv(main, null);
                Parser.Instance.Parse(main);
                FuncRuntime.MainRuntime.Read(mainEnv).Run(null);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        public static bool ConsoleCommand(string code)
        {
            switch (code)
            {
                case "#gomo":
                    KulaVersion.HelloKula();
                    return true;
            }

            return false;
        }

        public static void DebugREPL()
        {
            string code;
            while (true)
            {
                Console.Write(">> ");
                code = Console.ReadLine();
                if (code == "")
                {
                    continue;
                }
                if (code == "#exit")
                {
                    break;
                }
                else
                {
                    if (!ConsoleCommand(code))
                    {
                        DebugRunCode(code);
                    }
                }
            }
        }
    }
}
