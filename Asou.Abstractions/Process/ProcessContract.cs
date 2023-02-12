namespace Asou.Abstractions.Process;

public record ProcessContract(
    Guid ProcessContractId,
    Guid ProcessVersionId,
    int VersionNumber,
    string Name
);
