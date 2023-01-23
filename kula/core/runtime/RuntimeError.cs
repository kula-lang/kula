using Kula.Core.Ast;

namespace Kula.Core.Runtime;

class RuntimeError : Exception {
    public readonly Token? name;

    public RuntimeError(Token name, string msg) : base(msg) {
        this.name = name;
    }

    public RuntimeError(string msg) : base(msg) { }
}
