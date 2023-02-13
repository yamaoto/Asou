using System.Text.Json;
using Asou.Abstractions.Container;
using Asou.Abstractions.Process.Execution;
using Microsoft.EntityFrameworkCore;

namespace Asou.EfCore.ProcessPersistence;

public class ProcessExecutionPersistenceEfCoreRepository : IProcessExecutionPersistenceRepository
{
    private readonly DbContext _dbContext;
    private readonly DbSet<ProcessParameterPersistentModel> _processParameterPersistent;

    public ProcessExecutionPersistenceEfCoreRepository(DbContextResolver dbContextResolver)
    {
        _dbContext = dbContextResolver.GetContext();
        _processParameterPersistent = _dbContext.Set<ProcessParameterPersistentModel>();
    }

    public async Task StoreProcessParameterAsync(Guid instanceId, Dictionary<string, ValueContainer> parameters,
        CancellationToken cancellationToken = default)
    {
        var existing = await _processParameterPersistent
            .FirstOrDefaultAsync(f => f.ProcessInstanceId == instanceId, cancellationToken);
        if (existing == null)
        {
            existing = new ProcessParameterPersistentModel(instanceId);
            existing.JsonContent = JsonSerializer.Serialize(parameters);
            await _processParameterPersistent.AddAsync(existing, cancellationToken);
        }
        else
        {
            existing.JsonContent = JsonSerializer.Serialize(parameters);
            _processParameterPersistent.Update(existing);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Dictionary<string, ValueContainer>> GetProcessParametersAsync(Guid instanceId,
        CancellationToken cancellationToken = default)
    {
        var existing = await _processParameterPersistent
            .FirstOrDefaultAsync(f => f.ProcessInstanceId == instanceId, cancellationToken);
        if (existing != null)
        {
            return JsonSerializer.Deserialize<Dictionary<string, ValueContainer>>(existing.JsonContent)!;
        }

        return new Dictionary<string, ValueContainer>();
    }
}
