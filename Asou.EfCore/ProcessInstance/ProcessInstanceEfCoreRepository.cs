using Asou.Abstractions.Process.Instance;
using Microsoft.EntityFrameworkCore;

namespace Asou.EfCore.ProcessInstance;

public class ProcessInstanceEfCoreRepository : IProcessInstanceRepository
{
    private readonly DbContext _dbContext;
    private readonly DbSet<ProcessInstanceModel> _processInstances;

    public ProcessInstanceEfCoreRepository(DbContextResolver dbContextResolver)
    {
        _dbContext = dbContextResolver.GetContext();
        _processInstances = _dbContext.Set<ProcessInstanceModel>();
    }

    public async Task CreateInstanceAsync(Guid id, Guid processContractId, Guid processVersionId, int version,
        PersistenceType persistenceType, ProcessInstanceState state, CancellationToken cancellationToken = default)
    {
        var processInstance = new ProcessInstanceModel(id, DateTime.UtcNow, processContractId, processVersionId,
            version,
            persistenceType) { State = state };

        await _processInstances.AddAsync(processInstance, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateStateInstanceAsync(Guid id, ProcessInstanceState state,
        CancellationToken cancellationToken = default)
    {
        var processInstance = await _processInstances.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
        if (processInstance != null)
        {
            processInstance.State = state;
            _processInstances.Update(processInstance);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IEnumerable<ProcessInstanceModel>> GetRunningInstancesAsync(
        CancellationToken cancellationToken = default)
    {
        var processInstances = await _processInstances
            .Where(w => w.State == ProcessInstanceState.Running)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        return processInstances;
    }

    public async Task<IEnumerable<ProcessInstanceModel>> GetAllInstancesAsync(
        CancellationToken cancellationToken = default)
    {
        var processInstances = await _processInstances
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        return processInstances;
    }

    public async Task<ProcessInstanceModel?> GetInstanceAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var processInstance = await _processInstances
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
        return processInstance;
    }
}
