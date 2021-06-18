using System;

namespace Kula.Util
{
    public static class KulaException
    {
        // 底层错误
        public class LexerException : Exception
        {
            public LexerException() : base("Incomplete Code.") { }
        }
        public class ParserException : Exception
        {
            public ParserException() : base("Syntax Error.") { }
        }
        
        // 表面错误
        public class VariableException : Exception
        {
            public VariableException() : base("Use Variable Before Init.") { }
        }

        // Func Exception
        public class ArgsTypeException : Exception
        {
            public ArgsTypeException() : base("Wrong Arguments Type.") { }
        }
        public class FuncUsingException : Exception
        {
            public FuncUsingException() : base("It is not a Func.") { }
        }
        public class FuncArgumentException : Exception
        {
            public FuncArgumentException() : base("Wrong Arguments Type.") { }
        }
        public class VMOverflowException : Exception
        {
            public VMOverflowException() : base("Out of Memory.") { }
        }
        public class VMUnderflowException : Exception
        {
            public VMUnderflowException() : base("Wrong usage of Func?") { }
        }

        // Lambda
        public class ReturnValueException : Exception
        {
            public ReturnValueException() : base("Wrong Return Value Type.") { }
        }

        // Array Exception
        public class ArrayTypeException : Exception
        {
            public ArrayTypeException() : base("Wrong Type in Usage of Array.") { }
        }
        public class ArrayIndexException : Exception
        {
            public ArrayIndexException() : base("Array Index out of range.") { }
        }

        // Map Exception
        public class MapTypeException : Exception
        {
            public MapTypeException() : base("Wrong Type in Usage of Map.") { }
        }
        public class MapKeyException : Exception
        {
            public MapKeyException() : base("Key Not in this Map.") { }
        }

        // 自定义 异常信息
        public class UserException : Exception
        {
            public UserException(string msg) : base(msg) { }
        }
    }
}
