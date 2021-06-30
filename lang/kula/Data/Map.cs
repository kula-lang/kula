using System.Collections.Generic;
using System.Text;

using Kula.Util;

namespace Kula.Data
{
    /// <summary>
    /// Kula 中的 Map 结构
    /// </summary>
    public class Map
    {
        private readonly SortedDictionary<string, object> data;

        /// <summary>
        /// 获取 实质数据
        /// </summary>
        public SortedDictionary<string, object> Data { get => data; }

        /// <summary>
        /// 构造函数 生成 Kula 中的 Map
        /// </summary>
        public Map() { data = new SortedDictionary<string, object>(); }

        /// <summary>
        /// 转化为 字符串 JSON
        /// </summary>
        /// <returns>JSON</returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('{');
            foreach (KeyValuePair<string, object> kvp in data)
            {
                if (builder.Length != 1) builder.Append(',');
                builder.Append(
                    '\"' + kvp.Key + '\"' + ':' + kvp.Value.KToString()
                );
            }
            builder.Append('}');
            return builder.ToString();
        }
    }
}
