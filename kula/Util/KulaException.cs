using System;

namespace Kula.Util
{
    public static class KulaException
    {
        // 底层错误
        public class LexerException : Exception
        {
            public LexerException() : base("LexerException : Incomplete Code.") { }
        }
        public class ParserException : Exception
        {
            public ParserException() : base("ParserException : Syntax Error.") { }
        }
        
        // 表面错误
        public class VariableException : Exception
        {
            public VariableException() : base("VariableException : Use Variable Before Init.") { }
        }

        // Func Exception
        public class FuncTypeException : Exception
        {
            public FuncTypeException() : base("FuncTypeException : Wrong Arguments Type.") { }
        }
        public class FuncUsingException : Exception
        {
            public FuncUsingException() : base("FuncUsingException : It is not a Func.") { }
        }
        public class FuncArgumentException : Exception
        {
            public FuncArgumentException() : base("FuncArgumentException : Wrong Arguments Type.") { }
        }
        public class VMOverflowException : Exception
        {
            public VMOverflowException() : base("VM-OverflowException : Out of Memory.") { }
        }
        public class VMUnderflowException : Exception
        {
            public VMUnderflowException() : base("VM-UnderflowException : Wrong usage of Func?") { }
        }

        // Lambda
        public class ReturnValueException : Exception
        {
            public ReturnValueException() : base("ReturnValueException : Wrong Return Value Type.") { }
        }

        // Array Exception
        public class ArrayTypeException : Exception
        {
            public ArrayTypeException() : base("ArrayTypeException : Wrong Type in Usage of Array.") { }
        }
        public class ArrayIndexException : Exception
        {
            public ArrayIndexException() : base("ArrayIndexException : Array Index out of range.") { }
        }

        // Map Exception
        public class MapTypeException : Exception
        {
            public MapTypeException() : base("MapTypeException : Wrong Type in Usage of Map.") { }
        }
        public class MapKeyException : Exception
        {
            public MapKeyException() : base("MapKeyException : Key Not in this Map.") { }
        }

        // 自定义 异常信息
        public class UserException : Exception
        {
            public UserException(string msg) : base("UserException : " + msg) { }
        }
    }
}
