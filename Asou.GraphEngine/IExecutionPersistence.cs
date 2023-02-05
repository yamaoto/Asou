namespace Asou.GraphEngine;

public interface IExecutionPersistence
{
    Task SaveExecutionStatus(Guid processContractId, Guid processVersionId, Guid instanceId, Guid threadId,
        string elementName, int state);

    // TODO: Prepare feature for persistence of parameters

    // TODO: Get stored thread,element and state for restoring execution
}