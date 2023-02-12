using Asou.Abstractions.Events;

namespace Asou.Abstractions.Process;

public interface IProcessInstance
{
    public IProcessRuntime ProcessRuntime { get; }
    public ProcessContract ProcessContract { get; }
    public Guid Id { get; }
    public PersistType PersistType { get; init; }

    Task HandleSubscriptionEventAsync(EventSubscriptionModel subscription, EventRepresentation eventRepresentation,
        CancellationToken cancellationToken = default);
}
