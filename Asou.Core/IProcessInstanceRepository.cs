namespace Asou.Core;

public interface IProcessInstanceRepository
{
    Task CreateInstance(Guid processInstanceId, Guid processContractId, Guid processVersionId, int versionNumber,
        int state);

    Task UpdateInstance(Guid processInstanceId, Guid processContractId, Guid processVersionId, int versionNumber,
        int state);
}