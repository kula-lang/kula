using System;
using System.Collections.Generic;
using System.Diagnostics;

using kula.Core;
using kula.Data;

namespace kula.Util
{
    static class ConsoleUtility
    {
        public static readonly Dictionary<LexTokenType, ConsoleColor> LexColorDict = new Dictionary<LexTokenType, ConsoleColor>()
        {
            { LexTokenType.KEYWORD, ConsoleColor.Red },
            { LexTokenType.TYPE, ConsoleColor.Yellow },
            { LexTokenType.NAME, ConsoleColor.Cyan },
            { LexTokenType.NUMBER, ConsoleColor.Blue },
            { LexTokenType.STRING, ConsoleColor.Magenta },
            { LexTokenType.SYMBOL, ConsoleColor.Green },
        };
        public static readonly Dictionary<KvmNodeType, ConsoleColor> KvmColorDict = new Dictionary<KvmNodeType, ConsoleColor>()
        {
            { KvmNodeType.VALUE, ConsoleColor.Blue },
            { KvmNodeType.LAMBDA, ConsoleColor.DarkBlue },
            { KvmNodeType.STRING, ConsoleColor.Blue },
            { KvmNodeType.VARIABLE, ConsoleColor.Cyan },
            { KvmNodeType.NAME, ConsoleColor.Cyan },
            { KvmNodeType.FUNC, ConsoleColor.Magenta },
            { KvmNodeType.IFGOTO, ConsoleColor.Red },
            { KvmNodeType.GOTO, ConsoleColor.Red },

            { KvmNodeType.VECTERKEY, ConsoleColor.Yellow },
        };

        private static readonly KulaVersion version = new KulaVersion("Crow Bite", 0, new DateTime(2021, 6, 4));
        public static void HelloKula()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(version + " (on .net Framework at least 4.6)");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("developed by @HanaYabuki in github.com");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("https://github.com/HanaYabuki/Kula");
            Console.ResetColor();
        }
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
            Console.WriteLine("\tIt takes "+ stopwatch.Elapsed.Milliseconds + " ms.\n");
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
                    HelloKula();
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
