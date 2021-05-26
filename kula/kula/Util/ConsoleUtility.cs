using System;
using System.IO;
using System.Collections.Generic;

using kula.Core;
using kula.Core.VMObj;

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
            Console.WriteLine("Kula - Ice Coffin - 0 [2021/5/26] (on .net Framework at least 4.6)");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("developed by @HanaYabuki in github.com");
            Console.ResetColor();
        }
        public static void DebugRunCode(string code)
        {
            try
            {
                KulaVM.Instance.Read(
                    Parser.Instance.Read(
                        Lexer.Instance.Read(code).Scan().Show().Out()
                    ).Parse().Show().Out()
                ).DebugRun();
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
                KulaVM.Instance.Read(
                    Parser.Instance.Read(
                        Lexer.Instance.Read(code).Scan().Out()
                    ).Parse().Out()
                ).Run();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public static void DebugREPL()
        {
            string code;
            while (true)
            {
                Console.Write(">> ");
                code = Console.ReadLine();
                if (code == "#exit")
                {
                    break;
                }
                DebugRunCode(code);
            }
        }
    }
}
