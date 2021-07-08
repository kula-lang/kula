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
        /// <summary>
        /// 对所有 容器内元素 的 字符串转化 
        /// 形成类似 JSON 格式的字符串
        /// </summary>
        /// <param name="_this">元素</param>
        /// <returns>类JSON格式字符串</returns>
        public static string KToString(this object _this)
        {
            if (_this == null) { return "null"; }
            else if (_this is string)
            {
                return "\"" + _this + "\"";
            }
            else if (_this is BuiltinFunc)
            {
                return Parser.InvertTypeDict[typeof(BuiltinFunc)];
            }
            return _this.ToString();
        }

        /// <summary>
        /// 将 Kula 语言支持的类型转化为对应的 字符串
        /// </summary>
        /// <param name="_this">Kula支持的类型</param>
        /// <returns>对应字符串</returns>
        public static string KTypeToString(this Type _this)
        {
            if (_this == null)
            {
                return "None";
            }
            if (Parser.InvertTypeDict.ContainsKey(_this))
            {
                return Parser.InvertTypeDict[_this];
            }
            throw new KulaException.KTypeException(_this.Name);
        }
    }
}
