using System;
using System.Collections.Generic;

using Kula.Util;

namespace Kula.Core
{
    struct VMNode
    {
        VMNodeType type;
        object value;

        public VMNode(VMNodeType type, object value)
        {
            this.type = type;
            this.value = value;
        }

        public VMNodeType Type { get => type; set => type = value; }
        public object Value { get => value; set => this.value = value; }

        public override string ToString()
        {
            string str_type = type.ToString(), str_value = value.ToString();
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

            { VMNodeType.CON_KEY, ConsoleColor.Yellow },
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

        CONKEY,    // 右值索引
    }
}
