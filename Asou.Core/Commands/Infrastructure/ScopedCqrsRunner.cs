using Microsoft.Extensions.DependencyInjection;

namespace Asou.Core.Commands.Infrastructure;

public class ScopedCqrsRunner
{
    private readonly IServiceProvider _serviceProvider;

    public ScopedCqrsRunner(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public CqrsRunnerScope<TCommand> Get<TCommand>() where TCommand : notnull
    {
        var scope = _serviceProvider.CreateScope();
        var scopedHandler = new CqrsRunnerScope<TCommand>(scope, scope.ServiceProvider.GetRequiredService<TCommand>());
        return scopedHandler;
    }

    public class CqrsRunnerScope<TCommand> : IDisposable
    {
        private readonly IServiceScope _scope;

        internal CqrsRunnerScope(IServiceScope scope, TCommand commandHandler)
        {
            _scope = scope;
            Handler = commandHandler;
        }

        public TCommand Handler { get; }

        public void Dispose()
        {
            _scope.Dispose();
        }
    }
}
