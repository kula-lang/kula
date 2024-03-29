namespace Kula.Core.Runtime;

public class NativeFunction : ICallable
{
    public delegate object? Callee(object? _this, List<object?> arguments);

    private int arity;
    private Callee callee;
    private object? callSite;

    public int Arity => arity;

    public object? Call(List<object?> arguments)
    {
        object? callsite = callSite;
        Unbind();
        return callee(callsite, arguments);
    }

    public void Bind(object? callSite)
    {
        this.callSite = callSite;
    }

    void Unbind()
    {
        this.callSite = null;
    }

    public NativeFunction(int arity, Callee callee)
    {
        this.callee = callee;
        this.arity = arity;
    }

    public override string ToString()
    {
        return "<Function>";
    }
}