namespace Kula.Core.Runtime;

class RuntimeError : Exception {
    public readonly Token name;

    public RuntimeError(Token name, string msg) : base(msg) {
        this.name = name;
    }
}