using Kula;
using System.IO;

class Program {
    public static void Main(string[] args) {
        KulaEngine kula = new KulaEngine();
        kula.RunProject("demo");

        Console.ReadLine();
        return;
    }
}