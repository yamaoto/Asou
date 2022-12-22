using Asou.Abstractions.ExecutionElements;

namespace Asou.Abstractions;

public interface IProcessMachine
{
    string Name { get; init; }
    IReadOnlyDictionary<string, BaseElement> Components { get; }
    IReadOnlyDictionary<string, object?> Parameters { get; }
    IReadOnlyDictionary<string, Action> Procedures { get; }
}