using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kula.Data
{
    class Map
    {
        private readonly SortedDictionary<string, object> data;

        public Map() { data = new SortedDictionary<string, object>(); }

        public object this[string key]
        {
            get 
            { 
                if (data.ContainsKey(key))
                {
                    return data[key];
                }
                throw new Util.KulaException.MapKeyException();
            }
            set { data[key] = value; }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('{');
            foreach(KeyValuePair<string, object> kvp in data)
            {
                if (builder.Length != 1) builder.Append(',');
                builder.Append(
                    '\"' + kvp.Key + '\"' + ':' + 
                    (kvp.Value.GetType() == typeof(string) ? 
                        ('\"' + kvp.Value.ToString() + '\"') 
                        : kvp.Value )
                );
            }
            builder.Append('}');
            return builder.ToString();
        }
    }
}
