namespace Asou.Core;

public interface IProcessContractRepository
{
    Task<ProcessContract?> GetActiveProcessContractAsync(Guid processContractId);
    Task<ProcessContract?> GetProcessContractAsync(Guid processContractId, Guid processVersionId, int versionNumber);
}