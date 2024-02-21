using Microsoft.Extensions.DependencyInjection;

namespace Asou.Core.Commands.Infrastructure;

public class ScopedActionRunner
{
    private readonly IServiceProvider _serviceProvider;

    public ScopedActionRunner(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ActionRunnerScope<TCommand> Get<TCommand>() where TCommand : notnull
    {
        var scope = _serviceProvider.CreateScope();
        var scopedHandler =
            new ActionRunnerScope<TCommand>(scope, scope.ServiceProvider.GetRequiredService<TCommand>());
        return scopedHandler;
    }

    public class ActionRunnerScope<TCommand> : IDisposable
    {
        private readonly IServiceScope _scope;

        internal ActionRunnerScope(IServiceScope scope, TCommand commandHandler)
        {
            _scope = scope;
            Handler = commandHandler;
        }

        public TCommand Handler { get; }

        public void Dispose()
        {
            _scope.Dispose();
        }

        public T Get<T>() where T : notnull
        {
            return _scope.ServiceProvider.GetRequiredService<T>();
        }
    }
}
