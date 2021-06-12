using System;
using System.Collections.Generic;

using Kula.Util;

namespace Kula.Core
{
    struct LexToken
    {
        private readonly LexTokenType type;
        private readonly string value;

        public string Value { get => value; }
        public LexTokenType Type { get => type; }

        public LexToken(LexTokenType type, string value)
        {
            this.type = type;
            this.value = value;
        }

        public override string ToString()
        {
            string str_type = type.ToString();
            return ""
                    + "< "
                    + str_type.PadRight(9)
                    + "| "
                    + value.PadRight(18)
                    + " >";
        }


        // 颜色表
        public static readonly Dictionary<LexTokenType, ConsoleColor> LexColorDict = new Dictionary<LexTokenType, ConsoleColor>()
        {
            { LexTokenType.NAME, ConsoleColor.Cyan },
            { LexTokenType.NUMBER, ConsoleColor.Blue },
            { LexTokenType.STRING, ConsoleColor.Magenta },
            { LexTokenType.SYMBOL, ConsoleColor.Green },
        };
    }

    enum LexTokenType : byte
    {
        NAME,       // 名字，可解析为 关键字 类型名 变量名 函数名
        NUMBER,     // 数字，float
        STRING,     // 字符串，string
        SYMBOL,     // 符号
    }
}
