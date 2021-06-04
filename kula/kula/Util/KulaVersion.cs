using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kula.Util
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
    }
}
