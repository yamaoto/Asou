namespace Asou.Core.Interpreter;

public interface IByteCodeInterpreter
{
    Task<Instructions> EvaluateNextAsync(CancellationToken cancellationToken = default);
    Task RunAsync(CancellationToken cancellationToken = default);
}