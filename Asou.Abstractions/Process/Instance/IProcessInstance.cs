using Asou.Abstractions.Events;
using Asou.Abstractions.Process.Contract;
using Asou.Abstractions.Process.Execution;

namespace Asou.Abstractions.Process.Instance;

public interface IProcessInstance
{
    public IProcessRuntime ProcessRuntime { get; }
    public ProcessContract ProcessContract { get; }
    public Guid Id { get; }
    public PersistenceType PersistenceType { get; }

    Task HandleSubscriptionEventAsync(EventSubscriptionModel subscription, EventRepresentation eventRepresentation,
        CancellationToken cancellationToken = default);
}
