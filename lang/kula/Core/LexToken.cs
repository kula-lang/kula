using System;
using System.Collections.Generic;

namespace Kula.Core
{
    struct LexToken
    {
        public string Value { get; }
        public LexTokenType Type { get; }
        public int LineNum { get; }

        public LexToken(LexTokenType type, string value, int lineNum)
        {
            this.Type = type;
            this.Value = value;
            this.LineNum = lineNum;
        }

        public override string ToString()
        {
            return $"< {Type,-9} | {Value,-18} | {LineNum,-6} >";
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
        NUMBER_OR_NAME,   // 数字或名字，识别'+' '-'时可能出现
        STRING,         // 字符串，string
        SYMBOL,         // 符号
    }
}
