using Asou.Core.Process;

namespace Asou.Core;

public interface IProcessInstance
{
    public ProcessRuntime ProcessRuntime { get; }
    public ProcessContract ProcessContract { get; }
    public Guid Id { get; }
    public PersistType PersistType { get; init; }
}