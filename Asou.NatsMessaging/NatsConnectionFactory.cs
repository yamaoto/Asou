using NATS.Client;

namespace Asou.NatsMessaging;

public class NatsConnectionFactory : INatsConnectionFactory
{
    private readonly ConnectionFactory _connectionFactory = new();
    private readonly Options _options;
    private IConnection? _connection;

    public NatsConnectionFactory(Options options)
    {
        _options = options;
    }

    public IConnection GetConnection()
    {
        _connection ??= _connectionFactory.CreateConnection(_options);
        return _connection;
    }
}
