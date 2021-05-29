using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kula.Util
{
    static class KulaException
    {
        public class ParserException : ApplicationException
        {
            public ParserException() : base("ParserException : Syntax Error.") { }
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
        public class UnknownNameException : ApplicationException
        {
            public UnknownNameException() : base("UnknownNameException : Not assigned before use") { }
        }
    }
}
