using Asou.GraphEngine;

namespace Asou.EfCore;

public class ExecutionPersistence : IExecutionPersistence
{
    public Task SaveExecutionStatus(Guid processContractId, Guid processVersionId, Guid instanceId, Guid threadId,
        Guid elementId, int state)
    {
        // TODO: Implement save execution status
        return Task.CompletedTask;
    }
}
