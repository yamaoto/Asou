using Asou.Abstractions;

namespace Asou.InMemory;

public class InMemoryMessageQueue : TapQueue<QueueMessage>
{
}

public record QueueMessage(string Queue, object Message);
