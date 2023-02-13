namespace Asou.Abstractions.Process.Instance;

public record ProcessInstance(
    Guid ProcessContractId,
    Guid ProcessVersionId,
    int VersionNumber,
    int State
);
