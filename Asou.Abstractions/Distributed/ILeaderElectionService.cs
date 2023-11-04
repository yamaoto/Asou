namespace Asou.Abstractions.Distributed;

public interface ILeaderElectionService
{
    public Task<string> GetLeaderAsync(CancellationToken cancellationToken = default);
}
