using System;

using Kula.Data;
using Kula.Core;

namespace Kula.Util
{
    /// <summary>
    /// 用于扩展方法等
    /// </summary>
    static class KulaUtil
    {
        public static string ToString(this BuiltinFunc _this)
        {
            return "<builtin-func/>";
        }

        /// <summary>
        /// 对所有 容器内元素 的 字符串转化
        /// </summary>
        /// <param name="_this">元素</param>
        /// <returns>字符串</returns>
        public static string KToString(this object _this)
        {
            if (_this == null) { return "null"; }
            else if (_this is string)
            {
                return "\"" + _this + "\"";
            }
            return _this.ToString();
        }

        public static string KTypeToString(this Type _this)
        {
            foreach (var kv in Parser.TypeDict)
            {
                if (_this == kv.Value)
                {
                    return kv.Key;
                }
            }
            throw new KulaException.KTypeException(_this.Name);
        }
    }
}
