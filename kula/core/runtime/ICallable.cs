namespace Kula.Core.Runtime;

interface ICallable {
    int Arity { get; }
    object? Call(List<object?> arguments);
    void Bind(object? callSite);
    void Unbind();
}
