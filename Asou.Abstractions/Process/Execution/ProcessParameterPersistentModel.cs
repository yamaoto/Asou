namespace Asou.Abstractions.Process.Execution;

public record ProcessParameterPersistentModel
(
    Guid ProcessInstanceId
)
{
    public required string JsonContent { get; set; }
}
