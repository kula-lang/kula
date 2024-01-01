namespace Kula.Core.Runtime;

class Environment
{
    private readonly Environment enclosing;
    private readonly Dictionary<string, object?> values = new Dictionary<string, object?>();

    public Environment()
    {
        enclosing = this;
    }

    public Environment(Environment enclosing)
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

        throw new RuntimeInnerError($"Undefined variable '{name}'.");
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

        throw new RuntimeInnerError($"Undefined variable '{name}' when assign.");
    }

    public void Define(string name, object? value)
    {
        values[name] = value;
    }
    
    public Environment Unenclose() {
        return this.enclosing;
    }
}
