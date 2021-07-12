using System;

namespace Kula.Util
{
    class KulaVersion
    {
        private readonly string name;
        private readonly string number;

        private readonly DateTime dateTime;

        private KulaVersion(string name, string number, DateTime dateTime)
        {
            this.name = name;
            this.number = number;
            this.dateTime = dateTime;
        }

        public string ToJson()
        {
            return "";
        }

        public override string ToString()
        {
            return "Kula - " + name + " - " + number + " [" + dateTime.Year + "/" + dateTime.Month + "/" + dateTime.Day + "]";
        }

        // 版本信息
        private static readonly KulaVersion version = new KulaVersion("Pre", "0.1.1", new DateTime(2021, 7, 12));
        public static KulaVersion Version { get => version; }
    }
}
