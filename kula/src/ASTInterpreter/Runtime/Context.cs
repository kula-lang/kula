namespace Kula.ASTInterpreter.Runtime;

class Context
{
    private readonly Context enclosing;
    private readonly Dictionary<string, object?> values = new Dictionary<string, object?>();

    public Context()
    {
        enclosing = this;
    }

    public Context(Context enclosing)
    {
        this.enclosing = enclosing;
    }

    public object? Get(string name)
    {
        if (values.ContainsKey(name)) {
            return values[name];
        }
        if (enclosing != this) {
            return enclosing.Get(name);
        }

        throw new InterpreterInnerException($"Undefined variable '{name}'.");
    }

    public void Assign(string name, object? value)
    {
        if (values.ContainsKey(name)) {
            values[name] = value;
            return;
        }
        if (enclosing != this) {
            enclosing.Assign(name, value);
            return;
        }

        throw new InterpreterInnerException($"Undefined variable '{name}' when assign.");
    }

    public void Define(string name, object? value)
    {
        values[name] = value;
    }
    
    public Context Unenclose() {
        return this.enclosing;
    }
}
