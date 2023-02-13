using Asou.Core;
using Microsoft.Extensions.Hosting;

namespace Asou;

public class AsouBackgroundServices : IHostedService
{
    private readonly ProcessExecutionEngine _processExecutionEngine;

    public AsouBackgroundServices(ProcessExecutionEngine processExecutionEngine)
    {
        _processExecutionEngine = processExecutionEngine;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _processExecutionEngine.InitializeAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _processExecutionEngine.StopExecutionAsync(cancellationToken);
    }
}
