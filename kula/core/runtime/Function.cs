using Kula.Core.Ast;

namespace Kula.Core.Runtime;

class Function : ICallable {
    private readonly Expr.Function defination;
    private readonly Interpreter interpreter;

    public Function(Expr.Function expr, Interpreter interpreter) {
        this.defination = expr;
        this.interpreter = interpreter;
    }

    public int Arity => defination.parameters.Count;

    object? ICallable.Call(List<object?> arguments) {
        Runtime.Environment environment = new Runtime.Environment(interpreter.environment);

        environment.Define(Token.MakeTemp("self"), this);
        for (int i=0; i<Arity; ++i) {
            environment.Define(defination.parameters[i].Item1, arguments[i]);
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
        List<string> items = new List<string>();
        foreach (var name_type in defination.parameters) {
            Ast.Type type = name_type.Item2;
            items.Add(type?.ToString() ?? "None");
        }
        return $"<{string.Join(',', items)}:{defination.returnType.lexeme}>";
    }
}