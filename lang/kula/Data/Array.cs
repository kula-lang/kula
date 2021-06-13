using System.Text;

namespace Kula.Data
{
    public class Array
    {
        private readonly object[] data;
        public object[] Data { get => data; }

        public Array(int size)
        {
            this.data = new object[size];
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
