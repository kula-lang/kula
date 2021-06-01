using System;

namespace kula.Util
{
    static class KulaException
    {
        public class ParserException : ApplicationException
        {
            public ParserException() : base("ParserException : Syntax Error.") { }
        }
        public class LexerException : ApplicationException
        {
            public LexerException() : base("LexerException : Incomplete Code.") { }
        }
        public class VariableException : ApplicationException
        {
            public VariableException() : base("VariableException : Use Variable Before Init.") { }
        }
        public class FuncException : ApplicationException
        {
            public FuncException() : base("FuncException : Wrong arguments type.") { }
        }
        public class VMOverflowException : ApplicationException
        {
            public VMOverflowException() : base("VMOverflowException : Out of memory") { }
        }
        public class VMUnderflowException : ApplicationException
        {
            public VMUnderflowException() : base("VMUnderflowException : Wrong usage of Func ?") { }
        }
        public class ReturnValueException : ApplicationException
        {
            public ReturnValueException() : base("ReturnValueException : Wrong Return Value Type.") { }
        }
    }
}
