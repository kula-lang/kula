namespace Kula.Core.Runtime;

public class NativeFunction : ICallable {
    public delegate object? Callee(object? _this, List<object?> arguments);

    private int arity;
    private Callee callee;
    private object? callSite;

    int ICallable.Arity => arity;

    object? ICallable.Call(List<object?> arguments) {
        try {
            return callee(callSite, arguments);
        }
        finally {
            Unbind();
        }
    }

    public void Bind(object? callSite) {
        this.callSite = callSite;
    }

    public void Unbind() {
        this.callSite = null;
    }

    public NativeFunction(int arity, Callee callee) {
        this.callee = callee;
        this.arity = arity;
    }
}