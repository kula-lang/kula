using System;
using System.Collections.Generic;

namespace Kula.Core
{
    struct ByteCode
    {
        public ByteCode(ByteCodeType type, object value)
        {
            this.Type = type;
            this.Value = value;
        }

        public ByteCodeType Type { get; }
        public object Value { get; }

        public override string ToString()
        {
            string str_type = Type.ToString(),
                str_value = Value == null ? "" : Value.ToString();
            return
                $"[ {str_type, -9} | {str_value, -18} ]";
        }

        // 颜色表
        public static readonly Dictionary<ByteCodeType, ConsoleColor> KvmColorDict = new Dictionary<ByteCodeType, ConsoleColor>()
        {
            { ByteCodeType.VALUE, ConsoleColor.Blue },
            { ByteCodeType.LAMBDA, ConsoleColor.DarkBlue },
            { ByteCodeType.STRING, ConsoleColor.Blue },
            { ByteCodeType.VAR, ConsoleColor.DarkYellow },
            { ByteCodeType.SET, ConsoleColor.Yellow },
            { ByteCodeType.NAME, ConsoleColor.Cyan },
            { ByteCodeType.FUNC, ConsoleColor.Magenta },
            { ByteCodeType.IFGOTO, ConsoleColor.Red },
            { ByteCodeType.GOTO, ConsoleColor.Red },

            { ByteCodeType.CONKEY, ConsoleColor.Yellow },
            { ByteCodeType.RETURN, ConsoleColor.DarkMagenta },

            // { VMNodeType.PIPE, ConsoleColor.Magenta },
        };
    }

    enum ByteCodeType : byte
    {
        VALUE,      // 常量值
        STRING,     // 常字符串
        LAMBDA,     // 函数表达式

        VAR,        // 声明赋值
        SET,        // 赋值

        NAME,       // 变量名
        FUNC,       // 运行 Lambda
        IFGOTO,     // 为零时跳转
        GOTO,       // 无条件跳转
        RETURN,     // 返回值

        CONKEY,     // 右值索引
    }
}
