using Asou.Abstractions.Process.Execution;
using Microsoft.EntityFrameworkCore;

namespace Asou.EfCore.ProcessPersistence;

public class ProcessExecutionLogEfCoreRepository : IProcessExecutionLogRepository
{
    private readonly DbContext _dbContext;
    private readonly DbSet<ProcessExecutionLogModel> _processExecutionLogs;

    public ProcessExecutionLogEfCoreRepository(DbContextResolver dbContextResolver)
    {
        _dbContext = dbContextResolver.GetContext();
        _processExecutionLogs = _dbContext.Set<ProcessExecutionLogModel>();
    }

    public async Task SaveExecutionStatusAsync(Guid processContractId, Guid processVersionId, Guid processInstanceId,
        Guid threadId, Guid elementId, int state, CancellationToken cancellationToken = default)
    {
        var processExecutionLog = new ProcessExecutionLogModel(0, processContractId, processVersionId,
            processInstanceId, threadId, elementId, state, DateTime.UtcNow);
        await _processExecutionLogs.AddAsync(processExecutionLog, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task FlushLogForProcessInstanceAsync(Guid processInstanceId,
        CancellationToken cancellationToken = default)
    {
        await _processExecutionLogs
            .Where(w => w.ProcessInstanceId == processInstanceId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProcessExecutionLogModel>> GetLogForProcessInstanceAsync(Guid processInstanceId,
        CancellationToken cancellationToken = default)
    {
        return await _processExecutionLogs
            .Where(w => w.ProcessInstanceId == processInstanceId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProcessExecutionLogModel>> GetThreadsAsync(Guid processInstanceId,
        CancellationToken cancellationToken = default)
    {
        var query = _processExecutionLogs.AsNoTracking()
            .Where(log => log.ProcessInstanceId == processInstanceId)
            .GroupBy(log => log.ThreadId)
            // ReSharper disable once SimplifyLinqExpressionUseMinByAndMaxBy
            .Select(g => g.OrderByDescending(o => o.CreatedOn).FirstOrDefault());
        var result = await query.ToListAsync(cancellationToken);
        return result;
    }
}
