namespace Asou.NatsMessaging;

public interface IMessageConsumer
{
    Task HandleMessageAsync(object message, CancellationToken cancellationToken);
}
