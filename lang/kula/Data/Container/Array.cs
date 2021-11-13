using System.Text;
using Kula.Data.Function;
using Kula.Util;

namespace Kula.Data.Container
{
    /// <summary>
    /// Kula 中的 Array 结构
    /// </summary>
    public class Array
    {
        /// <summary>
        /// 获取 实质数据
        /// </summary>
        public object[] Data { get; }

        /// <summary>
        /// 构造函数 生成 Kula 中的 Array
        /// </summary>
        /// <param name="size"></param>
        public Array(int size) { this.Data = new object[size]; }

        /// <summary>
        /// 构造函数 使用 传入的源数组 构建 Kula 数组
        /// </summary>
        /// <param name="data">源数组</param>
        public Array(object[] data)
        {
            this.Data = data;
        }


        /// <summary>
        /// 转化为字符串 JSON
        /// </summary>
        /// <returns>JSON</returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('[');
            for (int i = 0; i < Data.Length; ++i)
            {
                if (Data[i] is Func || Data[i] is SharpFunc) { }
                else
                {
                    if (builder.Length != 1) { builder.Append(','); }
                    builder.Append(Data[i].KToString());
                }
            }
            builder.Append(']');
            return builder.ToString();
        }
    }
}
