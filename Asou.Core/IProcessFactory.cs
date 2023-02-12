namespace Asou.Core;

public interface IProcessFactory
{
    public Task<IProcessInstance> CreateProcessInstance(Guid processInstanceId, ProcessContract processContract);
}
