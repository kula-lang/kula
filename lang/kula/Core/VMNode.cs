using System;
using System.Collections.Generic;

namespace Kula.Core
{
    struct VMNode
    {
        public VMNode(VMNodeType type, object value)
        {
            this.Type = type;
            this.Value = value;
        }

        public VMNodeType Type { get; }
        public object Value { get; }

        public override string ToString()
        {
            string str_type = Type.ToString(),
                str_value = Value == null ? "" : Value.ToString();
            return ""
                    + "[ "
                    + str_type.PadRight(9)
                    + "| "
                    + str_value.PadRight(18)
                    + " ]";
        }

        // 颜色表
        public static readonly Dictionary<VMNodeType, ConsoleColor> KvmColorDict = new Dictionary<VMNodeType, ConsoleColor>()
        {
            { VMNodeType.VALUE, ConsoleColor.Blue },
            { VMNodeType.LAMBDA, ConsoleColor.DarkBlue },
            { VMNodeType.STRING, ConsoleColor.Blue },
            { VMNodeType.VAR, ConsoleColor.DarkYellow },
            { VMNodeType.LET, ConsoleColor.Yellow },
            { VMNodeType.NAME, ConsoleColor.Cyan },
            { VMNodeType.FUNC, ConsoleColor.Magenta },
            { VMNodeType.IFGOTO, ConsoleColor.Red },
            { VMNodeType.GOTO, ConsoleColor.Red },

            { VMNodeType.CONKEY, ConsoleColor.Yellow },
            { VMNodeType.RETURN, ConsoleColor.DarkMagenta },

            { VMNodeType.PIPE, ConsoleColor.Magenta },
        };
    }

    enum VMNodeType : byte
    {
        VALUE,      // 常量值
        STRING,     // 常字符串
        LAMBDA,     // 匿名函数

        VAR,        // 声明赋值
        LET,        // 赋值

        NAME,       // 变量名
        FUNC,       // 运行 Lambda
        IFGOTO,     // 为零时跳转
        GOTO,       // 无条件跳转
        RETURN,     // 返回值

        CONKEY,     // 右值索引

        PIPE,       // 管道
    }
}
