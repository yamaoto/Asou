namespace Asou.Abstractions;

public interface IInitializeHook
{
    Task Initialize(CancellationToken cancellationToken = default);
}
