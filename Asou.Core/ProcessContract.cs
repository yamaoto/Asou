namespace Asou.Core;

public record ProcessContract(
    Guid ProcessContractId,
    Guid ProcessVersionId,
    int VersionNumber,
    string Name
)
{
}