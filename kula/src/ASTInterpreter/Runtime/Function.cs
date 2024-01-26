using Kula.ASTCompiler.Parser;

namespace Kula.ASTInterpreter.Runtime;

class Function : ICallable
{
    private readonly Expr.Function defination;
    private readonly Interpreter interpreter;
    private readonly Context parent;
    private object? callSite;

    public Function(Expr.Function expr, Interpreter interpreter, Context parent)
    {
        this.defination = expr;
        this.interpreter = interpreter;
        this.parent = parent;
    }

    public int Arity => defination.parameters.Count;

    public object? Call(List<object?> arguments)
    {
        Context environment = new(parent);

        environment.Define("self", this);
        if (callSite is not null) {
            environment.Define("this", callSite);
            Unbind();
        }

        int size = Arity >= 0 ? Arity : arguments.Count;
        for (int i = 0; i < size; ++i) {
            environment.Define(defination.parameters[i].lexeme, arguments[i]);
        }

        try {
            interpreter.ExecuteBlock(defination.body, environment);
        }
        catch (Interpreter.Return return_value) {
            return return_value.value;
        }

        return null;
    }

    public override string ToString()
    {
        return "<Function>";
    }

    public void Bind(object? callSite)
    {
        this.callSite = callSite;
    }

    public void Unbind()
    {
        this.callSite = null;
    }
}