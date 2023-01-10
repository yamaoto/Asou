using Asou.Core;

namespace WebApplication1;

public class ProcessInstanceRepository : IProcessInstanceRepository
{
    public Task CreateInstance(Guid processInstanceId, Guid processContractId, Guid processVersionId, int versionNumber,
        int state)
    {
        return Task.CompletedTask;
    }

    public Task UpdateInstance(Guid processInstanceId, Guid processContractId, Guid processVersionId, int versionNumber,
        int state)
    {
        return Task.CompletedTask;
    }
}