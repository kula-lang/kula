using System.Text;

using Kula.Util;

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
                builder.Append(data[i].KToString());
            }
            builder.Append(']');
            return builder.ToString();
        }
    }
}
