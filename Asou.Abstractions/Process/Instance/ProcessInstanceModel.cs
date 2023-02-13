namespace Asou.Abstractions.Process.Instance;

public record ProcessInstanceModel
(
    Guid Id,
    DateTime CreatedOn,
    Guid ProcessContractId,
    Guid ProcessVersionId,
    int Version,
    PersistenceType PersistenceType
)
{
    public ProcessInstanceState State { get; set; }
}
