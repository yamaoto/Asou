namespace Asou.Abstractions.Process.Contract;

public record ProcessContract(
    Guid ProcessContractId,
    Guid ProcessVersionId,
    int VersionNumber,
    string Name
);
