using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kula.Data
{
    public class Map
    {
        private readonly SortedDictionary<string, object> data;

        public SortedDictionary<string, object> Data { get => data; }
        public Map() { data = new SortedDictionary<string, object>(); }

        private static string KToString(object arg)
        {
            if (arg == null) { return "null"; }
            if (arg is string)
            {
                return "\"" + arg + "\"";
            }
            if (arg is BuiltinFunc)
            {
                return "<builtin-func/>";
            }
            return arg.ToString();
        }
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('{');
            foreach(KeyValuePair<string, object> kvp in data)
            {
                if (builder.Length != 1) builder.Append(',');
                builder.Append(
                    '\"' + kvp.Key + '\"' + ':' + KToString(kvp.Value)
                );
            }
            builder.Append('}');
            return builder.ToString();
        }
    }
}
