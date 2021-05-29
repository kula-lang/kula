using System;
using System.Collections.Generic;
using System.IO;
using kula.Core;
using kula.Data;
using kula.Util;

namespace kula
{
    class Program
    {
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
                ConsoleUtility.HelloKula();
                ConsoleUtility.DebugREPL();
            }
            /*
            List<LexToken> lexTokens = Lexer.Instance.Read("foo = func(x:Num, y:Num):None { println(x);println(y); }; foo(2, 3);").Scan().Show().Out();
            Main main = new Main(lexTokens);
            Parser.Instance.Parse(main).Show();
            KulaVM.Instance.Read(main).DebugRun();
            Console.ReadKey();
            */
        }
    }
}
