using System.Text;

namespace Kula.Data
{
    /// <summary>
    /// Kula 中的 Array 结构
    /// </summary>
    public class Array
    {
        private readonly object[] data;
        
        /// <summary>
        /// 获取 实质数据
        /// </summary>
        public object[] Data { get => data; }

        /// <summary>
        /// 构造函数 生成 Kula 中的 Array
        /// </summary>
        /// <param name="size"></param>
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

        /// <summary>
        /// 转化为字符串 JSON
        /// </summary>
        /// <returns>JSON</returns>
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
