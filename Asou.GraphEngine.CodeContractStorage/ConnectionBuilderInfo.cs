namespace Asou.GraphEngine.CodeContractStorage;

public class ConnectionBuilderInfo
{
    public Type? FromType { get; init; }
    public string? From { get; init; }
    public required string To { get; init; }
    public required Type ToType { get; init; }
    public IsCanNavigateDelegate? Condition { get; init; }
    public string? ConditionName { get; init; }
}