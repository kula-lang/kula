using System;
using System.Collections.Generic;
using System.IO;

using Kula;

class Program
{
    private static readonly KulaEngine kulaEngine = new KulaEngine();
    private static bool debugMode = false;
    private delegate void ConsoleFunction();
    private static readonly Dictionary<string, ConsoleFunction> ConsoleFunctionDict = new Dictionary<string, ConsoleFunction>() 
    {
        {"", () => { } },
        {"#debug", () => { debugMode = true; Console.WriteLine("Debug-Mode"); } },
        {"#release", () => { debugMode = false; Console.WriteLine("Release-Mode");} },
        {"#gomo", () => { Hello(); } },
        {"#clear", () => { kulaEngine.Clear(); } },
    };

    private static void Repl()
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
            else if (ConsoleFunctionDict.ContainsKey(code))
            {
                ConsoleFunctionDict[code]();
            }
            else
            {
                try
                {
                    kulaEngine.Compile(code, "", debugMode);
                    kulaEngine.Run("", debugMode);
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.GetType().ToString() + " : " + e.Message);
                    Console.ResetColor();
                }
            }
        }
    }

    public static void Hello()
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine(KulaEngine.Version + " (on .net Framework at least 4.6)");
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("developed by @HanaYabuki in github.com");

        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write("More Info on ");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("https://github.com/kula-lang/Kula");

        Console.ResetColor();
    }

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
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e);
                return;
            }
            try
            {
                kulaEngine.Compile(code, "", (args.Length >= 2 && args[1] == "--debug"));
                kulaEngine.Run("", debugMode);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e);
                Console.ResetColor();
            }
        }
        else
        {
            Hello();
            Repl();
        }
    }
}