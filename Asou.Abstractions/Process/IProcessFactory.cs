namespace Asou.Abstractions.Process;

public interface IProcessFactory
{
    public Task<IProcessInstance> CreateProcessInstance(Guid processInstanceId, ProcessContract processContract);
}
