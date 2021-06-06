using System;

namespace kula.Util
{
    static class KulaException
    {
        // 底层错误
        public class LexerException : ApplicationException
        {
            public LexerException() : base("LexerException : Incomplete Code.") { }
        }
        public class ParserException : ApplicationException
        {
            public ParserException() : base("ParserException : Syntax Error.") { }
        }
        
        // 表面错误
        public class VariableException : ApplicationException
        {
            public VariableException() : base("VariableException : Use Variable Before Init.") { }
        }
        public class FuncUsingException : ApplicationException
        {
            public FuncUsingException() : base("FuncUsingException : Wrong Arguments type.") { }
            public FuncUsingException(bool flag) : base("FuncUsingException : Wrong Arguments count.") { }
        }
        public class VMOverflowException : ApplicationException
        {
            public VMOverflowException() : base("VM-OverflowException : Out of Memory.") { }
        }
        public class VMUnderflowException : ApplicationException
        {
            public VMUnderflowException() : base("VM-UnderflowException : Wrong usage of Func?") { }
        }

        // Lambda
        public class ReturnValueException : ApplicationException
        {
            public ReturnValueException() : base("ReturnValueException : Wrong Return Value Type.") { }
        }

        // Array Exception
        public class ArrayTypeException : ApplicationException
        {
            public ArrayTypeException() : base("ArrayTypeException : Wrong Type in Using of Array.") { }
        }
        public class ArrayIndexException : ApplicationException
        {
            public ArrayIndexException() : base("ArrayIndexException : Array Index out of range.") { }
        }

        // Map Exception
        public class MapTypeException : ApplicationException
        {
            public MapTypeException() : base("MapTypeException : Wrong Type in Using of Map.") { }
        }
        public class MapKeyException : ApplicationException
        {
            public MapKeyException() : base("MapKeyException : Key Not in this Map") { }
        }

        // 自定义 异常信息
        public class UserException : ApplicationException
        {
            public UserException(string msg) : base("UserException : " + msg) { }
        }
    }
}
