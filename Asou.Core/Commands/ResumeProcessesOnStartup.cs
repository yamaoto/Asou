using Asou.Abstractions.Distributed;
using Asou.Abstractions.Events;

namespace Asou.Core.Commands;

public class ResumeProcessesOnStartup
{
    private readonly IEventBus _eventBus;
    private readonly GetProcessesForResumeRequest _getProcessesForResumeRequest;
    private readonly ILeaderElectionService _leaderElectionService;

    public ResumeProcessesOnStartup(GetProcessesForResumeRequest getProcessesForResumeRequest, IEventBus eventBus,
        ILeaderElectionService leaderElectionService)
    {
        _getProcessesForResumeRequest = getProcessesForResumeRequest;
        _eventBus = eventBus;
        _leaderElectionService = leaderElectionService;
    }

    public async Task ActivateAsync(CancellationToken cancellationToken = default)
    {
        var leader = await _leaderElectionService.GetLeaderAsync(cancellationToken);

        if (leader != _eventBus.CurrentNode)
        {
            return;
        }

        try
        {
            var processesToResume = await _getProcessesForResumeRequest.GetAsyncEnumerable(cancellationToken);
            // TODO: Set process state to "Resuming" for preventing double resume
            foreach (var instance in processesToResume)
            {
                var payload = new EventRepresentation(Guid.NewGuid().ToString(), "Asou", "ResumeProcess",
                    instance.Id.ToString(), DateTime.UtcNow, null);
                await _eventBus.PublishAsync(payload, cancellationToken);
            }
        }
        finally
        {
            await _leaderElectionService.ReleaseLeadershipAsync(cancellationToken);
        }
    }
}
