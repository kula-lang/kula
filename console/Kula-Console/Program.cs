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
        {"#gomo", () => { KulaEngine.Hello(); } },
        {"#clear", () => { kulaEngine.Clear(); } },
        {"#dequeue", () => {
            if (kulaEngine.EngineQueue.Count <= 0)
            {
                Console.WriteLine("KulaQueue is Empty");
            }
            else
            {
                Console.WriteLine(kulaEngine.EngineQueue.Dequeue());
            }
        } }
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
            KulaEngine.Hello();
            Repl();
        }
    }
}