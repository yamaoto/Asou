namespace Asou.GraphEngine.CodeContractStorage;

public class ConnectionBuilderInfo
{
    public Guid? FromElementId { get; init; }
    public Guid ToElementId { get; init; }
    public IsCanNavigateDelegate? Condition { get; init; }
    public string? ConditionName { get; init; }
}
