namespace Kula.Core.Runtime;

class Environment {
    private readonly Environment enclosing;
    private readonly Dictionary<string, object?> values = new Dictionary<string, object?>();

    public Environment() {
        enclosing = this;
    }

    public Environment(Environment enclosing) {
        this.enclosing = enclosing;
    }

    public object? Get(Token name) {
        if (values.ContainsKey(name.lexeme)) {
            return values[name.lexeme];
        }
        if (enclosing != this) {
            return enclosing.Get(name);
        }

        throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
    }

    public void Assign(Token name, object? value) {
        if (values.ContainsKey(name.lexeme)) {
            values[name.lexeme] = value;
            return;
        }
        if (enclosing != this) {
            enclosing.Assign(name, value);
            return;
        }

        throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
    }

    public void Define(Token name, object? value) {
        values[name.lexeme] = value;
    }
}