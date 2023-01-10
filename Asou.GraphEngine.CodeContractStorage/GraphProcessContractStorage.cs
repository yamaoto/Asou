namespace Asou.GraphEngine.CodeContractStorage;

public class GraphProcessContractStorage : Dictionary<Guid, GraphProcessVersionStorage>
{
    private readonly Dictionary<string, Guid> _nameMap = new();
    public IReadOnlyDictionary<string, Guid> NameMap => _nameMap;

    public void Add(GraphProcessContract graphProcessContract)
    {
        if (!ContainsKey(graphProcessContract.ProcessContract.ProcessContractId))
            Add(graphProcessContract.ProcessContract.ProcessContractId, new GraphProcessVersionStorage());

        _nameMap[graphProcessContract.ProcessContract.Name] =
            graphProcessContract.ProcessContract.ProcessContractId;

        this[graphProcessContract.ProcessContract.ProcessContractId].Add(graphProcessContract);
    }
}