using System.Threading.Channels;

namespace Asou.NatsMessaging;

public class NatsMessagingChannel
{
    private readonly Channel<QueueMessage> _channel = Channel.CreateUnbounded<QueueMessage>();
    public ChannelReader<QueueMessage> ChannelReader => _channel.Reader;
    public ChannelWriter<QueueMessage> ChannelWriter => _channel.Writer;
}
