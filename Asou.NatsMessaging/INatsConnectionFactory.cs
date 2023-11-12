using NATS.Client;

namespace Asou.NatsMessaging;

public interface INatsConnectionFactory
{
    IConnection GetConnection();
}
