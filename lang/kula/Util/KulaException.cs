using System;

namespace Kula.Util
{
    /// <summary>
    /// 静态 异常类
    /// </summary>
    public static class KulaException
    {
        // 底层错误

        /// <summary>
        /// 词法分析错误
        /// </summary>
        public class LexerException : Exception
        {
            /// <summary>
            /// （疑似不会出现
            /// </summary>
            public LexerException() : base("Incomplete Code.") { }
        }

        /// <summary>
        /// 语法分析错误
        /// </summary>
        public class ParserException : Exception
        {
            /// <summary>
            /// 语法错误
            /// </summary>
            public ParserException() : base("Syntax Error.") { }
        }
        
        // 表面错误

        /// <summary>
        /// 变量错误
        /// </summary>
        public class VariableException : Exception
        {
            /// <summary>
            /// 使用前未初始化
            /// </summary>
            /// <param name="name"></param>
            public VariableException(string name) 
                : base("Use Variable Before Init. => " + name) { }
        }

        // Func Exception
        /// <summary>
        /// 参数类型错误
        /// </summary>
        public class ArgsTypeException : Exception
        {
            /// <summary>
            /// 参数类型错误
            /// </summary>
            /// <param name="realType">传入的参数</param>
            /// <param name="needType">应为参数</param>
            public ArgsTypeException(string realType, string needType) 
                : base("Wrong Arguments Type. => " + needType + " but " + realType){ }
        }

        /// <summary>
        /// 函数使用错误
        /// </summary>
        public class FuncUsingException : Exception
        {
            /// <summary>
            /// 这不是函数
            /// </summary>
            /// <param name="funcName">疑似函数名</param>
            public FuncUsingException(string funcName) 
                : base("It is not a Func. => " + funcName) { }
        }

        /// <summary>
        /// 函数参数错误
        /// </summary>
        public class FuncArgumentException : Exception
        {
            /// <summary>
            /// 函数参数个数错误
            /// </summary>
            public FuncArgumentException() : base("Wrong Arguments Count.") { }
        }

        /// <summary>
        /// 下溢出
        /// </summary>
        public class VMUnderflowException : Exception
        {
            /// <summary>
            /// Kula 虚拟机栈 下溢出
            /// 疑似函数使用错误
            /// </summary>
            public VMUnderflowException() : base("Wrong usage of Func?") { }
        }

        // Lambda

        /// <summary>
        /// 返回值错误
        /// </summary>
        public class ReturnValueException : Exception
        {
            /// <summary>
            /// 返回值类型错误
            /// </summary>
            /// <param name="realType">传出的类型</param>
            /// <param name="needType">应为类型</param>
            public ReturnValueException(string realType, string needType) 
                : base("Wrong Return Value Type. => " + needType + " but " + realType){ }
        }

        // Array Exception

        /// <summary>
        /// Array 索引错误
        /// </summary>
        public class ArrayTypeException : Exception
        {
            /// <summary>
            /// 索引不为 Number
            /// </summary>
            public ArrayTypeException() : base("Wrong Type in Usage of Array.") { }
        }

        // Map Exception

        /// <summary>
        /// Map 键错误
        /// </summary>
        public class MapTypeException : Exception
        {
            /// <summary>
            /// 键不为 Str
            /// </summary>
            public MapTypeException() : base("Wrong Type in Usage of Map.") { }
        }

        // 自定义 异常信息

        /// <summary>
        /// 自定义异常
        /// </summary>
        public class UserException : Exception
        {
            /// <summary>
            /// 自定义异常
            /// </summary>
            /// <param name="msg"></param>
            public UserException(string msg) : base(msg) { }
        }
    }
}
