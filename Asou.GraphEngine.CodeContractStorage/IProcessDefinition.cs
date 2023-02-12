namespace Asou.GraphEngine.CodeContractStorage;

public interface IProcessDefinition
{
    public Guid Id { get; }
    public Guid VersionId { get; }
    public int Version { get; }

    public string Name { get; }
    public void Describe(GraphProcessContract builder);
}
