namespace Asou.NatsMessaging;

public class MessageConsumerRegistrations
{
    private readonly Dictionary<string, Type> _registrations = new();

    public void Add<T>(string queueName)
        where T : IMessageConsumer
    {
        _registrations.Add(queueName, typeof(T));
    }

    public IReadOnlyDictionary<string, Type> GetRegistrations()
    {
        return _registrations;
    }
}
