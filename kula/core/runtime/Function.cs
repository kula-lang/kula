using Kula.Core.Ast;

namespace Kula.Core.Runtime;

class Function : ICallable {
    private readonly Expr.Function defination;
    private readonly Interpreter interpreter;
    private readonly Environment parent;
    private object? @this;

    public Function(Expr.Function expr, Interpreter interpreter, Environment parent) {
        this.defination = expr;
        this.interpreter = interpreter;
        this.parent = parent;
    }

    public int Arity => defination.parameters.Count;

    object? ICallable.Call(List<object?> arguments) {
        Runtime.Environment environment = new Runtime.Environment(parent);

        environment.Define(Token.MakeTemp("self"), this);
        if (@this is not null) {
            environment.Define(Token.MakeTemp("this"), @this);
            Unbind();
        }

        int size = Arity >= 0 ? Arity : arguments.Count;
        for (int i = 0; i < size; ++i) {
            environment.Define(defination.parameters[i], arguments[i]);
        }

        try {
            interpreter.ExecuteBlock(defination.body, environment);
        }
        catch (Interpreter.Return return_value) {
            return return_value.value;
        }

        return null;
    }

    public override string ToString() {
        return "<Function>";
    }

    public void Bind(object? @this) {
        this.@this = @this;
    }

    public void Unbind() {
        this.@this = null;
    }
}