using Asou.Abstractions.Container;

namespace Asou.Abstractions.Process.Execution;

public interface IProcessExecutionPersistenceRepository
{
    Task StoreProcessParameterAsync(Guid instanceId, Dictionary<string, ValueContainer> parameters,
        CancellationToken cancellationToken = default);

    Task<Dictionary<string, ValueContainer>> GetProcessParametersAsync(Guid instanceId,
        CancellationToken cancellationToken = default);
}
