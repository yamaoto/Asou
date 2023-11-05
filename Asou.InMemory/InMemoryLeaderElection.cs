using Asou.Abstractions.Distributed;
using Asou.Abstractions.Events;

namespace Asou.InMemory;

public class InMemoryLeaderElection : ILeaderElectionService
{
    private readonly IEventBus _eventBus;

    public InMemoryLeaderElection(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public Task<string> GetLeaderAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_eventBus.CurrentNode);
    }

    public Task ReleaseLeadershipAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
