using System;

namespace Kula.Util
{
    class KulaVersion
    {
        private readonly string name;
        private readonly int count;

        private readonly DateTime dateTime;

        private KulaVersion(string name, int count, DateTime dateTime)
        {
            this.name = name;
            this.count = count;
            this.dateTime = dateTime;
        }

        public string ToJson()
        {
            return "";
        }

        public override string ToString()
        {
            return "Kula - " + name + " - " + count + " [" + dateTime.Year + "/" + dateTime.Month + "/" + dateTime.Day + "]";
        }

        // 版本信息
        private static readonly KulaVersion version = new KulaVersion("LTS", 0, new DateTime(2021, 6, 25));
        public static KulaVersion Version { get => version; }
    }
}
