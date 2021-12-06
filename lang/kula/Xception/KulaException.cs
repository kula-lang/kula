using Kula.Data.Type;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kula.Xception
{
    /// <summary>
    /// 词法分析错误
    /// </summary>
    public class LexerException : Exception
    {
        /// <summary>
        /// （疑似不会出现
        /// </summary>
        public LexerException(string msg)
            : base($"Incomplete Code. => {msg}") { }
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
        public ParserException(string msg) : base("Syntax Error => " + msg) { }

    }

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
            : base($"Use Variable Before Init. => {name}") { }
    }

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
            : base($"Wrong Arguments Type. => need {needType} but {realType} is given") { }
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
            : base($"It is not a Func. => {funcName}") { }
    }

    /// <summary>
    /// 函数参数错误
    /// </summary>
    public class FuncArgumentException : Exception
    {
        private static string TypeString(IType[] types)
        {
            string @string = "<";
            for (int i = 0; i < types.Length; ++i)
            {
                var tp = types[i];
                @string += tp.ToString() + (i != types.Length - 1 ? ", " : "");
            }
            @string += ">";
            return @string;
        }

        /// <summary>
        /// 函数参数个数错误
        /// </summary>
        public FuncArgumentException(string msg, IType[] types, int size)
            : base($"Wrong Arguments Count on {msg}. We need {size} arg(s) => {TypeString(types)}") { }

        public FuncArgumentException(string msg, IType[] types)
            : base($"Wrong Arguments Types in {msg}. We need => {TypeString(types)}") { }
    }

    /// <summary>
    /// 递归溢出
    /// </summary>
    public class OverflowException : Exception
    {
        /// <summary>
        /// Kula 递归调用深度超出限制
        /// </summary>
        public OverflowException()
            : base("Too deep recursion.") { }
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
        public VMUnderflowException(string msg)
            : base($"VM Underflow => {msg}") { }
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
            : base($"Wrong Return Value Type. => need {needType} but {realType} is given") { }
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
        public ArrayTypeException()
            : base("Wrong Type in Usage of Array.") { }
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
        public MapTypeException(string msg)
            : base($"Wrong Type in Usage of Map. => {msg}") { }
    }

    // 类型异常

    /// <summary>
    /// 类型异常 非 Kula 支持的类型
    /// </summary>
    public class KTypeException : Exception
    {
        /// <summary>
        /// 该类型不为 Kula 支持的类型
        /// </summary>
        /// <param name="type">类型名</param>
        public KTypeException(string type)
            : base($"It can not be regarded as a Kula Type => {type}") { }
        
        /// <summary>
        /// 该类型不为 Kula 类型，并且伴有难以描述的错误
        /// </summary>
        /// <param name="msg">错误信息</param>
        /// <param name="type">类型名</param>
        public KTypeException(string msg, string type)
            : base($"{msg} => {type}") { }
    }

    public class CommandException : Exception
    {
        public CommandException(string msg)
            : base($"No such command name => {msg}") { }
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
        public UserException(string msg)
            : base(msg) { }
    }
}
