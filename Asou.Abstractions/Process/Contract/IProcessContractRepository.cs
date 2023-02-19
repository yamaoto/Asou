namespace Asou.Abstractions.Process.Contract;

public interface IProcessContractRepository
{
    Task<ProcessContract?> GetActiveProcessContractAsync(Guid processContractId);
    Task<ProcessContract?> GetProcessContractAsync(Guid processContractId, Guid processVersionId, int versionNumber);
}
