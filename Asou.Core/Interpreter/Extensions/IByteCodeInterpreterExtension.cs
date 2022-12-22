using Asou.Abstractions;

namespace Asou.Core.Interpreter.Extensions;

public interface IByteCodeInterpreterExtension
{
    public Task ExecuteInstructionAsync(byte instruction, ByteCodeFormatReader reader,
        IProcessMachineCommands processMachineCommands,
        CancellationToken cancellationToken = default);
}