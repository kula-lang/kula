using Kula.Core.Ast;

namespace Kula.Core.Runtime;

class RuntimeError : Exception
{
    public readonly Token token;

    public RuntimeError(Token token, string msg) : base(msg)
    {
        this.token = token;
    }
}

class RuntimeInnerError : Exception
{
    public RuntimeInnerError(string msg) : base(msg) { }
}