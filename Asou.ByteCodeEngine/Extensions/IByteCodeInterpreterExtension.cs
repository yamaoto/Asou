using Asou.Abstractions;

namespace Asou.ByteCodeEngine.Extensions;

public interface IByteCodeInterpreterExtension
{
    public Task ExecuteInstructionAsync(byte instruction, ByteCodeFormatReader reader,
        IProcessMachineCommands processMachineCommands,
        CancellationToken cancellationToken = default);
}