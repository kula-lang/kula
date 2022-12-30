using Kula;
using System.IO;

class Program {
    public static void Main(string[] args) {
        KulaEngine kula = new KulaEngine();
        kula.Run(File.ReadAllText("./demo/test.kula"));

        Console.ReadLine();
        return;
    }
}