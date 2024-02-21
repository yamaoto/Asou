namespace Asou.Abstractions.Messaging;

public interface IMessagingService
{
    public string CurrentNode { get; }

    /// <summary>
    ///     Send message to queue
    /// </summary>
    /// <param name="queue">Queue</param>
    /// <param name="message">Message</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SendAddressedAsync<T>(string queue, T message,
        CancellationToken cancellationToken = default) where T : notnull;

    /// <summary>
    ///     Subscribe to a queue
    /// </summary>
    /// <param name="queue"></param>
    /// <param name="messageType"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    ///     Subscription object from messaging system provider
    /// </returns>
    Task<object> SubscribeAsync(string queue, Type messageType,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Cancel subscription
    /// </summary>
    /// <param name="queue">Queue</param>
    /// <param name="subscription">Subscription object</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CancelSubscriptionAsync(string queue, object subscription, CancellationToken cancellationToken = default);
}
