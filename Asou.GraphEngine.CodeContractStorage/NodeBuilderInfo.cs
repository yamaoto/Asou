namespace Asou.GraphEngine.CodeContractStorage;

public class NodeBuilderInfo
{
    public required string Name { get; init; }
    public required Type ElementType { get; init; }
    public required List<ParameterPersistenceInfo> Parameters { get; init; }
}