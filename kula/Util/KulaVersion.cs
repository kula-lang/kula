using System;

namespace Kula.Util
{
    class KulaVersion
    {
        private readonly string name;
        private readonly int count;

        private readonly DateTime dateTime;

        public KulaVersion(string name, int count, DateTime dateTime)
        {
            this.name = name;
            this.count = count;
            this.dateTime = dateTime;
        }

        public override string ToString()
        {
            return "Kula - " + name + " - " + count + " [" + dateTime.Year + "/" + dateTime.Month + "/" + dateTime.Day + "]";
        }


        // 版本信息
        private static readonly KulaVersion version = new KulaVersion("Diamond Breath LTS", 0, new DateTime(2021, 6, 8));
        public static void HelloKula()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(version + " (on .net Framework at least 4.6)");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("developed by @HanaYabuki in github.com");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("https://github.com/HanaYabuki/Kula");
            Console.ResetColor();
        }
    }
}
