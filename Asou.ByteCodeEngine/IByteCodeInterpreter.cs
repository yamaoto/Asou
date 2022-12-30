namespace Asou.ByteCodeEngine;

public interface IByteCodeInterpreter
{
    Task<Instructions> EvaluateNextAsync(CancellationToken cancellationToken = default);
    Task RunAsync(CancellationToken cancellationToken = default);
}