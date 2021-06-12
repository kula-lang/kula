using System.Text;

namespace Kula.Data
{
    class Array
    {
        private readonly object[] data;

        public Array(int size)
        {
            this.data = new object[size];
        }

        public object this[int index]
        {
            get 
            { 
                if (index < data.Length && index >= 0)
                {
                    return data[index];
                }
                throw new Util.KulaException.ArrayIndexException();
            }
            set 
            {
                if (index < data.Length && index >= 0)
                {
                    data[index] = value;
                }
                else
                {
                    throw new Util.KulaException.ArrayIndexException();
                }
            }
        }
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
            builder.Append('[');
            for (int i = 0; i < data.Length; ++i)
            {
                if (builder.Length != 1) { builder.Append(','); }
                builder.Append(KToString(data[i]));
            }
            builder.Append(']');
            return builder.ToString();
        }
    }
}
