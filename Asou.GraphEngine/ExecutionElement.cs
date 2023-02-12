namespace Asou.GraphEngine;

public struct ExecutionElement
{
    public ExecutionElement(Guid elementId, Guid threadId, int executionState)
    {
        ElementId = elementId;
        ThreadId = threadId;
        ExecutionState = executionState;
    }

    public Guid ElementId { get; init; }
    public Guid ThreadId { get; init; }
    public int ExecutionState { get; init; }
}
