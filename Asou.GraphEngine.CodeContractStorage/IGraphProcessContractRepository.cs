using Asou.Abstractions.Process.Contract;

namespace Asou.GraphEngine.CodeContractStorage;

public interface IGraphProcessContractRepository : IProcessContractRepository
{
    GraphProcessContract? GetGraphProcessContract(Guid processContractId, Guid processVersionId, int versionNumber);
    void AddProcessContract(GraphProcessContract graphProcessContract);
}
