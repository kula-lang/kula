// #define KULA_DEBUG

using Kula;
using Kula.Data.Function;
using Kula.Data.Type;
using System;
using System.Collections.Generic;

/**
 * 这里偷偷开一个 TODO-LIST 嘿嘿
 * 
 * 数据结构初始化构造器语法
 * 不定长参数
 * 语法树？
 * 文档
 */
class Program
{
    private static readonly KulaEngine kulaEngine = new();
    private static readonly Repl repl = new();

    private delegate void ShellCommand(bool flag);

    private static readonly Dictionary<string, ShellCommand> ShellCommands = new()
    {
        ["debug"] = (flag) => { kulaEngine.SetMode(flag ? 0xffff : 0x0); },
        ["lexer"] = (flag) => { kulaEngine.UpdateMode(flag, KulaEngine.Config.LEXER); },
        ["parser"] = (flag) => { kulaEngine.UpdateMode(flag, KulaEngine.Config.PARSER); },
        ["stop-watch"] = (flag) => { kulaEngine.UpdateMode(flag, KulaEngine.Config.STOP_WATCH); },
        ["value-stack"] = (flag) => { kulaEngine.UpdateMode(flag, KulaEngine.Config.VALUE_STACK); },
        ["repl-echo"] = (flag) => { kulaEngine.UpdateMode(flag, KulaEngine.Config.REPL_ECHO); },
        ["type-check"] = (flag) => { kulaEngine.UpdateMode(flag, KulaEngine.Config.TYPE_CHECK); },
        ["gomo"] = (_) => { Gomo(); },
        ["clear"] = (_) => { kulaEngine.Clear(); },
    };

    static Program() { }

    private static void Main(string[] args)
    {
        // 测试 Call
        kulaEngine.ExtendFunc["KULA_CALL"] = new SharpFunc((args, engine) => {
            return kulaEngine.Call(args[0], ((Kula.Data.Container.Array)args[1]).Data);
        }, RawType.Func, RawType.Array);

        if (args.Length > 0)
        {
            EnvRun(args);
        }
        else
        {
            Repl();
        }
    }

    private static void EnvRun(string[] args)
    {
        string code_path = null;
        foreach (string arg in args)
        {
            if (arg.StartsWith("--"))
            {
                try
                {
                    RunCommand(arg.Replace("--", ""), false);
                }
                catch (Kula.Xception.CommandException e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine(e.Message);
                    Console.ResetColor();
                    return;
                }
            }
            else
            {
                code_path = arg;
            }
        }

        if (code_path == null)
        {
            Repl();
        }
        else
        {
#if KULA_DEBUG
            kulaEngine.CompileFile(code_path, "");
            kulaEngine.Run("");
#else
            try
            {
                kulaEngine.CompileFile(code_path, "");
                kulaEngine.Run("");
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(e.Message);
                Console.ResetColor();
                return;
            }
#endif
        }
    }

    private static void Repl()
    {
        Gomo();

        string code;

        // REPL 模式，默认打开回显和类型检查
        kulaEngine.UpdateMode(true, KulaEngine.Config.REPL_ECHO, KulaEngine.Config.TYPE_CHECK);

        while (true)
        {
            // Console.Write("\n>> ");
            // code = Console.ReadLine(); // 可以在此拓展REPL
            code = repl.ReadCode();
            if (code == null || code == "#exit\n")
            {
                break;
            }
            else if (code.StartsWith("#"))
            {
                RunCommand(code.Replace("#", "").Replace(" ", "").Replace("\n", ""), true);
            }
            else
            {
#if KULA_DEBUG
                kulaEngine.CompileCode(code, "");
                kulaEngine.Run("");
#else
                try
                {
                    kulaEngine.CompileCode(code, "");
                    kulaEngine.Run("");
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine(e.GetType().ToString() + ":\n" + e.Message);
                    Console.ResetColor();
                }
#endif
            }
        }
    }

    private static void Gomo()
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

    private static void RunCommand(string command, bool show)
    {
        string[] console_command = command.Split('=', ' ');
        if (ShellCommands.ContainsKey(console_command[0]))
        {
            bool console_flag = true;
            if (console_command.Length > 1)
            {
                string console_command_flag = console_command[1];
                if (console_command_flag == "false" || console_command_flag == "off" || console_command_flag == "0")
                {
                    console_flag = false;
                }
            }
            if (show)
            {
                Console.WriteLine($"{console_command[0]} = {(console_flag ? "on" : "off")}");
            }
            ShellCommands[console_command[0]](console_flag);
        }
        else
        {
            if (!show)
            {
                throw new Kula.Xception.CommandException(console_command[0]);
            }
        }
    }
}
