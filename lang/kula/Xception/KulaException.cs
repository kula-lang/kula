using Kula.Data.Type;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kula.Xception
{
    public abstract class KulaException : Exception 
    {
        public KulaException(string msg) : base(msg) { }
    }


    /// <summary>
    /// 词法分析错误
    /// </summary>
    public class LexerException : KulaException
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
    public class ParserException : KulaException
    {
        /// <summary>
        /// 语法错误
        /// </summary>
        public ParserException(string msg) : base($"Syntax Error. => {msg}") { }
        public ParserException(string msg, int line) : base($"Syntax Error. => {msg} | in line {line + 1}") { }

    }

    /// <summary>
    /// 变量错误
    /// </summary>
    public class VariableException : KulaException
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
    public class ArgsTypeException : KulaException
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
    public class FuncUsingException : KulaException
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
    public class FuncArgumentException : KulaException
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
    public class OverflowException : KulaException
    {
        /// <summary>
        /// Kula 递归调用深度超出限制
        /// </summary>
        public OverflowException()
            : base("Too deep recursion.") { }
    }

    /// <summary>
    /// 容器溢出
    /// </summary>
    public class ContainerException : KulaException
    {
        public ContainerException(int pos, int len)
            : base($"Array Index out of range. => {pos} : {len}") { }

        public ContainerException(string key)
            : base($"Map Key not found. => \"{key}\"") { }
    }

    /// <summary>
    /// 下溢出
    /// </summary>
    public class VMUnderflowException : KulaException
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
    public class ReturnValueException : KulaException
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
    public class ArrayTypeException : KulaException
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
    public class MapTypeException : KulaException
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
    public class KTypeException : KulaException
    {
        /// <summary>
        /// 该类型不为 Kula 支持的类型
        /// </summary>
        /// <param name="type">类型名</param>
        public KTypeException(string type)
            : base($"It can not be regarded as a Kula Type => {type}") { }
    }

    public class CommandException : KulaException
    {
        public CommandException(string msg)
            : base($"No such command name => {msg}") { }
    }


    // 自定义 异常信息

    /// <summary>
    /// 自定义异常
    /// </summary>
    public class UserException : KulaException
    {
        /// <summary>
        /// 自定义异常
        /// </summary>
        /// <param name="msg"></param>
        public UserException(string msg)
            : base(msg) { }
    }
}
