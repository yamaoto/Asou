using Asou.GraphEngine;

namespace WebApplication1;

public class ExecutionPersistence : IExecutionPersistence
{
    public Task SaveExecutionStatus(Guid processContractId, Guid processVersionId, Guid instanceId, Guid threadId,
        string elementName, int state)
    {
        return Task.CompletedTask;
    }
}