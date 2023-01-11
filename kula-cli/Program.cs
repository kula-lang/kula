using Kula;
using System.IO;

class Program {
    public static void Main(string[] args) {
        KulaEngine kula = new KulaEngine();
        kula.DebugRun(File.ReadAllText("demo/class.kula"));

        Console.ReadLine();
        return;
    }
}