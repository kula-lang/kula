#define KULA_DEBUG

using Kula;
using Kula.Data.Function;
using System;
using System.Collections.Generic;


class Program
{
    private static readonly KulaEngine kulaEngine = new();

    private delegate void ShellCommand();
    private static readonly Dictionary<string, ShellCommand> ShellCommandDict = new()
    {
        ["#debug"] = () =>
        {
            kulaEngine.SetMode(0xffff);
            Console.WriteLine("DEBUG-MODE");
        },
        ["#release"] = () =>
        {
            kulaEngine.SetMode(0);
            Console.WriteLine("RELEASE-MODE");
        },
        ["#lexer"] = () =>
        {
            kulaEngine.UpdateMode(KulaEngine.Config.LEXER);
            Console.WriteLine("LEXER " + (kulaEngine.CheckMode(KulaEngine.Config.LEXER) ? "on" : "off"));
        },
        ["#parser"] = () =>
        {
            kulaEngine.UpdateMode(KulaEngine.Config.PARSER);
            Console.WriteLine("PARSER " + (kulaEngine.CheckMode(KulaEngine.Config.PARSER) ? "on" : "off"));
        },
        ["#stop-watch"] = () =>
        {
            kulaEngine.UpdateMode(KulaEngine.Config.STOP_WATCH);
            Console.WriteLine("STOP-WATCH " + (kulaEngine.CheckMode(KulaEngine.Config.STOP_WATCH) ? "on" : "off"));
        },
        ["#value-stack"] = () =>
        {
            kulaEngine.UpdateMode(KulaEngine.Config.VALUE_STACK);
            Console.WriteLine("VALUE-STACK " + (kulaEngine.CheckMode(KulaEngine.Config.VALUE_STACK) ? "on" : "off"));
        },
        ["#repl-echo"] = () =>
        {
            kulaEngine.UpdateMode(KulaEngine.Config.REPL_ECHO);
            Console.WriteLine("REPL-ECHO " + (kulaEngine.CheckMode(KulaEngine.Config.REPL_ECHO) ? "on" : "off"));
        },
        ["#type-check"] = () =>
        {
            kulaEngine.UpdateMode(KulaEngine.Config.TYPE_CHECK);
            Console.WriteLine("TYPE-CHECK " + (kulaEngine.CheckMode(KulaEngine.Config.TYPE_CHECK) ? "on" : "off"));
        },
        ["#gomo"] = () => { Hello(); },
        ["#clear"] = () => { kulaEngine.Clear(); },
    };

    private delegate void ShellArgument();
    private static readonly Dictionary<string, ShellArgument> ShellArgumentDict = new()
    {
        ["--debug"] = () =>
        {
            kulaEngine.SetMode(0xffff);
            kulaEngine.UpdateMode(KulaEngine.Config.REPL_ECHO);
        },
        ["--release"] = () =>
        {
            kulaEngine.SetMode(0);
        },
        ["--stop-watch"] = () => { kulaEngine.UpdateMode(KulaEngine.Config.STOP_WATCH); },
        ["--value-stack"] = () => { kulaEngine.UpdateMode(KulaEngine.Config.VALUE_STACK); },
        ["--lexer"] = () => { kulaEngine.UpdateMode(KulaEngine.Config.LEXER); },
        ["--parser"] = () => { kulaEngine.UpdateMode(KulaEngine.Config.PARSER); },
        ["--type-check"] = () => { kulaEngine.UpdateMode(KulaEngine.Config.TYPE_CHECK); },
    };

    static Program()
    {
        ShellArgumentDict["-r"] = ShellArgumentDict["--release"];
        ShellArgumentDict["-d"] = ShellArgumentDict["--debug"];
    }

    private static void Repl()
    {
        string code;

        // REPL 模式
        kulaEngine.SetMode(KulaEngine.Config.REPL_ECHO);

        while (true)
        {
            Console.Write("\n>> ");
            code = Console.ReadLine();
            if (code == null || code == "#exit")
            {
                break;
            }
            else if (ShellCommandDict.ContainsKey(code))
            {
                ShellCommandDict[code]();
            }
            else
            {
                try
                {
                    kulaEngine.CompileCode(code, "");
                    kulaEngine.Run("");
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.GetType().ToString() + ":\n" + e.Message);
                    Console.ResetColor();
                }
            }
        }
    }

    private static void Hello()
    {
        Console.ForegroundColor = ConsoleColor.Magenta;

        Console.WriteLine($"{KulaEngine.Version} (on {KulaEngine.FrameworkVersion})");

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("developed by @HanaYabuki on github.com");

        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write("More Info - ");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("https://github.com/kula-lang/Kula");

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Copyright (C) 2021");

        Console.ResetColor();
    }

    private static void Main(string[] args)
    {
        // 测试
        kulaEngine.ExtendFunc["KULA_CALL"] = new SharpFunc((args, engine) =>
        {
            return kulaEngine.Call(args[0], ((Kula.Data.Container.Array)args[1]).Data);
        });

        if (args.Length > 0)
        {
            string code;
            foreach (string arg in args)
            {
                if (arg.StartsWith("-") && ShellArgumentDict.ContainsKey(arg))
                {
                    ShellArgumentDict[arg]();
                }
                else
                {
#if KULA_DEBUG
                    kulaEngine.CompileFile(arg, "");
                    kulaEngine.Run("");
#else
                    try
                    {
                        kulaEngine.CompileFile(arg, "");
                        kulaEngine.Run("");
                        // code = File.ReadAllText(arg);
                        // CompileAndRun(code);
                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(e.Message);
                        Console.ResetColor();
                        return;
                    }
#endif
                }
            }

        }
        else
        {
            Hello();
            Repl();
        }
    }
}