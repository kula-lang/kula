using Kula;

class Program {
    public static void Main(string[] args) {
        switch (args.Length) {
            case 0:
                Console.WriteLine("Usage: kula-cli <PATH> [options]");
                Repl();
                return;
            default:
                KulaEngine kula = new KulaEngine();
                FileInfo root = new FileInfo(args[0]);
                kula.Run(root);
                return;
        }
    }

    private static void Repl() {
        KulaEngine kula = new KulaEngine();
        string? source;
        for (; ; ) {
            Console.Write(">> ");
            source = Console.ReadLine();
            if (source is null || source.Trim() == "#exit") {
                break;
            }
            else {
                kula.Run(source);
            }
            Console.WriteLine();
        }
    }
}