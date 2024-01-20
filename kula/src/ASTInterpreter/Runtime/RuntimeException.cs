using Kula.ASTCompiler.Lexer;

namespace Kula.ASTInterpreter.Runtime;

class InterpreterException : Exception
{
    public readonly Token token;

    public InterpreterException(Token token, string msg) : base(msg)
    {
        this.token = token;
    }
}

class InterpreterInnerException : Exception
{
    public InterpreterInnerException(string msg) : base(msg) { }
}