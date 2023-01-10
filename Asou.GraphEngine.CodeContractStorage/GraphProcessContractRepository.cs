using Asou.Core;

namespace Asou.GraphEngine.CodeContractStorage;

public class GraphProcessContractRepository : IGraphProcessContractRepository
{
    private readonly GraphProcessContractStorage _storage = new();

    public Task<ProcessContract?> GetActiveProcessContractAsync(Guid processContractId)
    {
        if (_storage.TryGetValue(processContractId, out var items))
        {
            var graphProcessContract = items.MaxBy(m => m.ProcessContract.VersionNumber);
            if (graphProcessContract != null)
                return Task.FromResult((ProcessContract?)graphProcessContract.ProcessContract);
        }

        return Task.FromResult((ProcessContract?)null);
    }

    public Task<ProcessContract?> GetProcessContractAsync(Guid processContractId, Guid processVersionId,
        int versionNumber)
    {
        if (_storage.TryGetValue(processContractId, out var items))
        {
            var graphProcessContract = items.FirstOrDefault(m =>
                m.ProcessContract.ProcessVersionId == processVersionId &&
                m.ProcessContract.VersionNumber == versionNumber);
            if (graphProcessContract != null)
                return Task.FromResult((ProcessContract?)graphProcessContract.ProcessContract);
        }

        return Task.FromResult((ProcessContract?)null);
    }

    public GraphProcessContract? GetGraphProcessContract(Guid processContractId, Guid processVersionId,
        int versionNumber)
    {
        if (_storage.TryGetValue(processContractId, out var items))
        {
            var graphProcessContract = items.FirstOrDefault(m =>
                m.ProcessContract.ProcessVersionId == processVersionId &&
                m.ProcessContract.VersionNumber == versionNumber);
            if (graphProcessContract != null) return graphProcessContract;
        }

        return null;
    }

    public void AddProcessContract(GraphProcessContract graphProcessContract)
    {
        _storage.Add(graphProcessContract);
    }
}