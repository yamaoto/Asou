namespace Asou.Core;

public record ProcessInstance(
    Guid ProcessContractId,
    Guid ProcessVersionId,
    int VersionNumber,
    int State
)
{
}