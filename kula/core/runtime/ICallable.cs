namespace Kula.Core.Runtime;

public interface ICallable {
    int Arity { get; }
    object? Call(List<object?> arguments);
}