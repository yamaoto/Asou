namespace Asou.Abstractions.ExecutionElements;

public abstract class BaseElement
{
    public abstract string ClassName { get; init; }
    public abstract Task ExecuteAsync(CancellationToken cancellationToken = default);
}