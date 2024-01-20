namespace Kula.ASTInterpreter.Runtime;

interface ICallable
{
    int Arity { get; }
    object? Call(List<object?> arguments);
    void Bind(object? callSite);
}
