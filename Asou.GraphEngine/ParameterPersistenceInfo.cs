using Asou.Core;

namespace Asou.GraphEngine;

public class ParameterPersistenceInfo
{
    public required string Name { get; init; }
    public required Type Type { get; init; }
    public required Func<IProcessInstance, object>? Getter { get; init; }
    public required Action<IProcessInstance, object>? Setter { get; init; }
}