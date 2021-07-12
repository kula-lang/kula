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
        /// <summary>
        /// 获取 实质数据
        /// </summary>
        public SortedDictionary<string, object> Data { get; }

        /// <summary>
        /// 构造函数 生成 Kula 中的 Map
        /// </summary>
        public Map() 
        { 
            Data = new SortedDictionary<string, object>(); 
        }

        /// <summary>
        /// 构造函数 使用传入的源数据 构造 Kula中的 Map
        /// </summary>
        /// <param name="data"></param>
        public Map(SortedDictionary<string, object> data)
        {
            this.Data = data;
        }

        /// <summary>
        /// 转化为 字符串 JSON
        /// </summary>
        /// <returns>JSON</returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('{');
            foreach (KeyValuePair<string, object> kvp in Data)
            {
                if (kvp.Value is FuncWithEnv || kvp.Value is BFunc) { }
                else
                {
                    if (builder.Length != 1) builder.Append(',');
                    builder.Append(
                        '\"' + kvp.Key + '\"' + ':' + kvp.Value.KToString()
                    );
                }
            }
            builder.Append('}');
            return builder.ToString();
        }
    }
}
