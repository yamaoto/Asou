namespace Asou.Abstractions.Process;

public record ProcessInstance(
    Guid ProcessContractId,
    Guid ProcessVersionId,
    int VersionNumber,
    int State
);
