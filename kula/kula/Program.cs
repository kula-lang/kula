using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using kula.Core;
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
                    Console.WriteLine("打不开" + e.Message);
                    return;
                }
                ConsoleUtility.DebugRunCode(code);
            }
            else
            {
                ConsoleUtility.HelloKula();
                ConsoleUtility.DebugREPL();
            }
        }
    }
}
