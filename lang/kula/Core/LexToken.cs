using System;
using System.Collections.Generic;

namespace Kula.Core
{
    struct LexToken
    {
        public string Value { get; }
        public LexTokenType Type { get; }

        public LexToken(LexTokenType type, string value)
        {
            this.Type = type;
            this.Value = value;
        }

        public override string ToString()
        {
            string str_type = Type.ToString();
            return ""
                    + "< "
                    + str_type.PadRight(9)
                    + "| "
                    + Value.PadRight(18)
                    + " >";
        }


        // 颜色表
        public static readonly Dictionary<LexTokenType, ConsoleColor> LexColorDict = new Dictionary<LexTokenType, ConsoleColor>()
        {
            { LexTokenType.NAME, ConsoleColor.Green },
            { LexTokenType.NUMBER, ConsoleColor.Blue },
            { LexTokenType.STRING, ConsoleColor.Red },
            { LexTokenType.SYMBOL, ConsoleColor.DarkYellow },
        };
    }

    enum LexTokenType : byte
    {
        NULL,           // 无属性
        NAME,           // 名字，可解析为 关键字 类型名 变量名 函数名
        NUMBER,         // 数字，float
        NUMBERORNAME,   // 数字或名字，识别'+' '-'时可能出现
        STRING,         // 字符串，string
        SYMBOL,         // 符号
    }
}
