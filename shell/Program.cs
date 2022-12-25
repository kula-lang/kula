using Kula;
using System.IO;

class Program {
    public static void Main(string[] args) {
        Console.WriteLine("Hello Kula.");
        
        KulaEngine kula = new KulaEngine();
        kula.Run(File.ReadAllText("./demo/test.kula"));

        Console.ReadLine();
        return;
    }
}