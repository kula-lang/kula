using Kula.ASTInterpreter.Runtime;

namespace Kula.BytecodeInterpreter.Runtime;

class VMFunction : ICallable
{
    private readonly int index;
    private readonly Context parent;
    private object? callSite;

    public int Fp => index;
    public object? CallSite => callSite;
    public Context Parent => parent;

    public VMFunction(int index, Context parent)
    {
        this.index = index;
        this.parent = parent;
    }

    public int Arity => throw new NotImplementedException();

    public void Bind(object? callSite)
    {
        this.callSite = callSite;
    }

    public object? Call(List<object?> arguments)
    {
        throw new Exception();
    }

    public override string ToString()
    {
        return "<Function>";
    }
}