using System;

using Kula.Core;
using Kula.Data.Type;

namespace Kula.Util
{
    /// <summary>
    /// 用于扩展方法等
    /// </summary>
    static class StringUtil
    {
        /// <summary>
        /// 对所有 容器内元素 的 字符串转化 
        /// 形成类似 JSON 格式的字符串
        /// </summary>
        /// <param name="_this">元素</param>
        /// <returns>类JSON格式字符串</returns>
        public static string KToString(this object _this)
        {
            if (_this == null)
                return "null";
            else if (_this is string)
                return "\"" + _this + "\"";
            return _this.ToString();
        }
    }
}
