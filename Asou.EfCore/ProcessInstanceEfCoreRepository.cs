using Asou.Core;

namespace Asou.EfCore;

public class ProcessInstanceEfCoreRepository : IProcessInstanceRepository
{
    public Task CreateInstance(Guid processInstanceId, Guid processContractId, Guid processVersionId, int versionNumber,
        int state)
    {
        // TODO: Implement instance setting
        return Task.CompletedTask;
    }

    public Task UpdateInstance(Guid processInstanceId, Guid processContractId, Guid processVersionId, int versionNumber,
        int state)
    {
        // TODO: Implement instance setting
        return Task.CompletedTask;
    }
}
