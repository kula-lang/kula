namespace Kula.Core.Runtime;

interface ICallable {
    int Arity { get; }
    object? Call(List<object?> arguments);
    void Bind<T>(T? @this);
    void Unbind();
}
