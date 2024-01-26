using Kula;
using Kula.ASTInterpreter.Runtime;

class Program
{
    public static void Main(string[] args)
    {
        KulaEngine kula = new();
        if (args.Length == 0) {
            Info();
            Repl(kula);
            return;
        }
        else {
            switch (args[0]) {
                case "i":
                case "interpret": {
                        if (args.Length < 2) {
                            Info();
                        }
                        else {
                            FileInfo root = new(args[1]);
                            kula.Run(root);
                        }
                        return;
                    }
                case "c":
                case "compile": {
                        if (args.Length < 3) {
                            Info();
                        }
                        else {
                            FileInfo root = new(args[1]);
                            FileInfo aim = new(args[2]);
                            kula.Compile(root, aim);
                        }
                        return;
                    }
                case "r":
                case "run": {
                        if (args.Length < 2) {
                            Info();
                        }
                        else {
                            FileInfo root = new(args[1]);
                            kula.RunC(root);
                        }
                        return;
                    }
                case "cr":
                case "compile-run": {
                        if (args.Length < 3) {
                            Info();
                        }
                        else {
                            FileInfo root = new(args[1]);
                            FileInfo aim = new(args[2]);
                            kula.Compile(root, aim);
                            kula.RunC(aim);
                        }
                        return;
                    }
                default:
                    Info();
                    return;
            }
        }
    }

    private static void Info()
    {
        Console.WriteLine($"Kula-CLI (tags/v0.7.0) [.NET {Environment.Version} / {Environment.OSVersion}]");
        Console.WriteLine("Usage:\tkula-cli i|interpret <*.kula> [args...]");
        Console.WriteLine("      \tkula-cli c|compile <*.kula> <*.klc> [args...]");
        Console.WriteLine("      \tkula-cli r|run <*.klc> [args...]");
        Console.WriteLine("      \tkula-cli cr|compile-run <*.kula> <*.klc> [args...]");
    }

    private static void Repl(KulaEngine kula)
    {
        kula.DeclareFunction("kula", new NativeFunction(0, (_this, args) => {
            Console.ForegroundColor = ConsoleColor.Cyan;
            return "Diamond Breath!";
        }));
        kula.DeclareFunction("exit", new NativeFunction(0, (_this, args) => {
            Environment.Exit(0);
            return null;
        }));
        string? source;
        for (; ; ) {
            Console.WriteLine();
            source = InputMultiline();
            if (source is null) {
                break;
            }
            else {
                kula.Run(source);
            }
        }
    }

    private static string? InputMultiline()
    {
        int brackets = 0;
        char quote = '\0';

        string? ret = null;
        do {
            Console.Write(ret == null ? ">> " : ".. ");
            string? tmp = Console.ReadLine();
            if (tmp is null) {
                break;
            }
            else if (tmp.Trim() == "#break") {
                return "";
            }
            else {
                ret = (ret is null ? "" : (ret + Environment.NewLine)) + tmp;
                for (int i = 0; i < tmp.Length; ++i) {
                    if (quote == '\0') {
                        if (tmp[i] == '{' || tmp[i] == '[' || tmp[i] == '(') {
                            if (i > 0 && tmp[i - 1] == '\\') {

                            }
                            else {
                                brackets += 1;
                            }
                        }
                        else if (tmp[i] == '}' || tmp[i] == ']' || tmp[i] == ')') {
                            if (i > 0 && tmp[i - 1] == '\\') {

                            }
                            else {
                                brackets -= 1;
                            }
                        }
                        else if (tmp[i] == '"' || tmp[i] == '\'' || tmp[i] == '`') {
                            if (i > 0 && tmp[i - 1] == '\\') {
                            }
                            else {
                                quote = tmp[i];
                            }
                        }
                    }
                    else {
                        if (tmp[i] == quote) {
                            quote = '\0';
                        }
                    }
                }
            }
        }
        while (brackets > 0);
        return ret;
    }
}