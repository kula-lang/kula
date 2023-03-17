using Kula;

class Program
{
    public static void Main(string[] args)
    {
        switch (args.Length) {
            case 0:
                Info();
                Repl();
                return;
            default:
                KulaEngine kula = new KulaEngine();
                FileInfo root = new FileInfo(args[0]);
                kula.Run(root);
                return;
        }
    }

    private static void Info()
    {
        Console.WriteLine($"Kula-CLI (tags/v0.7.0) [.NET {Environment.Version} / {Environment.OSVersion}]");
        Console.WriteLine("Usage: kula-cli <PATH> [options]");
    }

    private static void Repl()
    {
        KulaEngine kula = new KulaEngine();
        kula.DeclareFunction("kula", new Kula.Core.Runtime.NativeFunction(0, (_this, args) => {
            Console.ForegroundColor = ConsoleColor.Cyan;
            return "Diamond Breath!";
        }));
        string? source;
        for (; ; ) {
            Console.WriteLine();
            source = InputMultiline();
            if (source is null || source.Trim() == "#exit") {
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