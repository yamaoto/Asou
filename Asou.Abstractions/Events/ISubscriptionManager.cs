namespace Asou.Abstractions.Events;

public interface ISubscriptionManager
{
    Task ReceiveEventAsync(EventRepresentation eventRepresentation,
        CancellationToken cancellationToken = default);
}
