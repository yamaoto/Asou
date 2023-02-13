namespace Asou.Abstractions.Process.Execution;

public record ProcessParameterPersistentModel
(
    Guid ProcessInstanceId
)
{
    public string JsonContent { get; set; }
}
