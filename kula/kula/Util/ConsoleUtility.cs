using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using kula.Core;
using kula.Core.VMObj;
using kula.DataObj;

namespace kula.Util
{
    static class ConsoleUtility
    {
        private static Dictionary<string, KvmBuiltinFunc>.KeyCollection func = Func.BuiltinFunc.Keys;
        private static HashSet<string> keyword = Lexer.Instance.KeyWord;
        private static HashSet<string> types = Lexer.Instance.Type;
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

        public static string Read()
        {
            StringBuilder ret = new StringBuilder("", 16);
            StringBuilder str = new StringBuilder("", 16);
            StringBuilder pre = new StringBuilder("", 16);
            ConsoleKeyInfo tmp;
            IEnumerator<string> i1 = func.GetEnumerator();
            IEnumerator<string> i2 = keyword.GetEnumerator();
            IEnumerator<string> i3 = types.GetEnumerator();
            bool searching = false;
            int cur = 1;
            tmp=Console.ReadKey();
            while (tmp.KeyChar != '\r')
            {
                if (tmp.KeyChar == '\t' && str.ToString() != "")
                {
                    searching = true;
                    if (cur == 1 && searching)
                    {
                        while (i1.MoveNext())
                        {
                            String k = i1.Current;
                            if (k.Length > str.ToString().Length && k.Substring(0, str.ToString().Length) == str.ToString())
                            {
                                searching = false;
                                Console.SetCursorPosition(3, Console.CursorTop);
                                Console.Write(new string(' ', Console.WindowWidth - 4 - ret.ToString().Length));
                                Console.SetCursorPosition(3, Console.CursorTop);
                                Console.Write(ret.ToString().Substring(0, ret.ToString().Length - str.ToString().Length) + k);
                                if (pre.ToString().Length == 0) pre.Append(k);
                                else pre.Replace(pre.ToString(), k);
                                break;
                            }
                        }
                        if (searching)
                        {
                            i1 = func.GetEnumerator();
                            cur++;
                        }
                    }
                    if(cur==2 && searching)
                    {
                        while (i2.MoveNext())
                        {
                            String k = i2.Current;
                            if (k.Length > str.ToString().Length && k.Substring(0, str.ToString().Length) == str.ToString())
                            {
                                searching = false;
                                Console.SetCursorPosition(3, Console.CursorTop);
                                Console.Write(new string(' ', Console.WindowWidth - 4 - ret.ToString().Length));
                                Console.SetCursorPosition(3, Console.CursorTop);
                                Console.Write(ret.ToString().Substring(0, ret.ToString().Length - str.ToString().Length) + k);
                                if (pre.ToString().Length == 0) pre.Append(k);
                                else pre.Replace(pre.ToString(), k);
                                break;
                            }
                        }
                        if (searching)
                        {
                            i2 = keyword.GetEnumerator();
                            cur++;
                        }
                    }
                    if (cur == 3 && searching)
                    {
                        while (i3.MoveNext())
                        {
                            String k = i3.Current;
                            if (k.Length > str.ToString().Length && k.Substring(0, str.ToString().Length) == str.ToString())
                            {
                                searching = false;
                                Console.SetCursorPosition(3, Console.CursorTop);
                                Console.Write(new string(' ', Console.WindowWidth - 4 - ret.ToString().Length));
                                Console.SetCursorPosition(3, Console.CursorTop);
                                Console.Write(ret.ToString().Substring(0, ret.ToString().Length - str.ToString().Length) + k);
                                if (pre.ToString().Length == 0) pre.Append(k);
                                else pre.Replace(pre.ToString(), k);
                                break;
                            }
                        }
                        if (searching)
                        {
                            i3 = types.GetEnumerator();
                            cur++;
                        }
                    }
                    if (cur == 4)
                    {
                        Console.SetCursorPosition(3, Console.CursorTop);
                        Console.Write(new string(' ', Console.WindowWidth - 4 - ret.ToString().Length));
                        Console.SetCursorPosition(3, Console.CursorTop);
                        Console.Write(ret.ToString());
                        cur = 1;
                    }
                }
                else if (tmp.KeyChar == 8)
                {
                    if (pre.ToString().Length > 0)
                    {
                        if (ret.ToString().Length - str.ToString().Length >= 0) ret.Replace(str.ToString(), pre.ToString(), ret.ToString().Length - str.ToString().Length, str.ToString().Length);
                        str.Replace(str.ToString(), pre.ToString());
                        pre.Replace(pre.ToString(), "");
                    }
                    if (str.ToString().Length > 0) str.Remove(str.ToString().Length - 1, 1);
                    if (ret.ToString().Length > 0) ret.Remove(ret.ToString().Length - 1, 1);
                    Console.SetCursorPosition(3, Console.CursorTop);
                    Console.Write(new string(' ', Console.WindowWidth - 4 - ret.ToString().Length));
                    Console.SetCursorPosition(3, Console.CursorTop);
                    Console.Write(ret.ToString());
                }
                else if(32<=tmp.KeyChar&&tmp.KeyChar<=126)
                {
                    if (pre.ToString().Length > 0)
                    {
                        if (ret.ToString().Length - str.ToString().Length >= 0) ret.Replace(str.ToString(), pre.ToString(), ret.ToString().Length - str.ToString().Length, str.ToString().Length);
                        str.Replace(str.ToString(), "");
                        pre.Replace(pre.ToString(), "");
                    }
                    if (('a' <= tmp.KeyChar && tmp.KeyChar <= 'z') || ('A' <= tmp.KeyChar && tmp.KeyChar <= 'Z')) str.Append(tmp.KeyChar);
                    else if ((tmp.KeyChar == ' ' || tmp.KeyChar == '=') && str.ToString().Length > 0) str.Replace(str.ToString(), "");
                    ret.Append(tmp.KeyChar);
                    Console.SetCursorPosition(3, Console.CursorTop);
                    Console.Write(new string(' ', Console.WindowWidth - 4 - ret.ToString().Length));
                    Console.SetCursorPosition(3, Console.CursorTop);
                    Console.Write(ret.ToString());
                }
                tmp = Console.ReadKey();
            }
            Console.WriteLine();
            return ret.ToString();
        }

        public static void DebugREPL()
        {
            string code;
            while (true)
            {
                Console.Write(">> ");
                code = Read();
                if (code == "#exit")
                {
                    break;
                }
                DebugRunCode(code);
            }
        }
    }
}
