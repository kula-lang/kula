using System;
using System.IO;

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
        }
    }
}
