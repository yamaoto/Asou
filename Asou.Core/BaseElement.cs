namespace Asou.Core;

public abstract class BaseElement
{
    public abstract Task ExecuteAsync(CancellationToken cancellationToken = default);
}