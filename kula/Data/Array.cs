using System.Text;

namespace kula.Data
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

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('[');
            for (int i = 0; i < data.Length; ++i)
            {
                if (builder.Length != 1) { builder.Append(','); }
                builder.Append(data[i] ?? "-");
            }
            builder.Append(']');
            return builder.ToString();
        }
    }
}
