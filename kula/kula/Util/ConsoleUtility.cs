using System;
using System.IO;
using System.Collections.Generic;

using kula.Core;
using kula.Data;

namespace kula.Util
{
    static class ConsoleUtility
    {
        public static Dictionary<LexTokenType, ConsoleColor> LexColorDict = new Dictionary<LexTokenType, ConsoleColor>()
        {
            { LexTokenType.KEYWORD, ConsoleColor.Red },
            { LexTokenType.TYPE, ConsoleColor.Yellow },
            { LexTokenType.NAME, ConsoleColor.Cyan },
            { LexTokenType.NUMBER, ConsoleColor.Blue },
            { LexTokenType.STRING, ConsoleColor.Magenta },
            { LexTokenType.SYMBOL, ConsoleColor.Green },
        };
        public static Dictionary<KvmNodeType, ConsoleColor> KvmColorDict = new Dictionary<KvmNodeType, ConsoleColor>()
        {
            { KvmNodeType.VALUE, ConsoleColor.Blue },
            { KvmNodeType.STRING, ConsoleColor.Blue },
            { KvmNodeType.VARIABLE, ConsoleColor.Cyan },
            { KvmNodeType.NAME, ConsoleColor.Cyan },
            { KvmNodeType.FUNC, ConsoleColor.Magenta },
            { KvmNodeType.IFGOTO, ConsoleColor.Red },
            { KvmNodeType.GOTO, ConsoleColor.Red },
        };

        public static void HelloKula()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Kula - One Inch - 1 [2021/5/30] (on .net Framework at least 4.6)");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("developed by @HanaYabuki in github.com");
            Console.ResetColor();
        }
        public static void DebugRunCode(string code)
        {
            try
            {
                List<LexToken> lexTokens = Lexer.Instance.Read(code).Scan().Show().Out();
                Func main = new Func(lexTokens);
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
        }
        public static void ReleaseRunCode(string code)
        {
            try
            {
                List<LexToken> lexTokens = Lexer.Instance.Read(code).Scan().Out();
                Func main = new Func(lexTokens);
                FuncEnv mainEnv = new FuncEnv(main, null);
                Parser.Instance.Parse(main);
                FuncRuntime.MainRuntime.Read(mainEnv).Run();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e);
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
                if (code == "#exit") 
                    break; 
                else 
                    if (!ConsoleCommand(code))
                        DebugRunCode(code);
            }
        }
    }
}
