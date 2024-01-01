using Kula.Core.Compiler;
using Kula.Core.Runtime;

namespace Kula.Core.VM;

class FunctionObject : ICallable
{
    private readonly int index;
    private readonly Runtime.Environment parent;
    private object? callSite;

    public int Fp => index;
    public object? CallSite => callSite;
    public Runtime.Environment Parent => parent;

    public FunctionObject(int index, Runtime.Environment parent)
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