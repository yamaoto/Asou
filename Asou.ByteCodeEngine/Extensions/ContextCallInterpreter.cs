using Asou.Abstractions;
using Microsoft.Extensions.Configuration;

namespace Asou.ByteCodeEngine.Extensions;

public class ContextCallInterpreter : IByteCodeInterpreterExtension
{
    public const byte ContextCallExtensionCode = 1;
    private readonly IConfiguration _configuration;

    public ContextCallInterpreter(
        IConfiguration configuration
    )
    {
        _configuration = configuration;
    }

    public Task ExecuteInstructionAsync(byte instruction, ByteCodeFormatReader reader,
        IProcessMachineCommands processMachineCommands,
        CancellationToken cancellationToken = default)
    {
        var callContextInstruction = (ContextCallInstructions)instruction;
        switch (callContextInstruction)
        {
            case ContextCallInstructions.ConstLiteral:
            {
                var literalType = reader.ReadType();
                var literalValue = reader.ReadValue(literalType);
                var parameterName = reader.ReadString();
                processMachineCommands.SetParameter(parameterName, literalType, literalValue);
            }
                break;
            case ContextCallInstructions.GetConfig:
            {
                var configPath = reader.ReadString();
                var parameterName = reader.ReadString();
                var value = _configuration[configPath];
                processMachineCommands.SetParameter(parameterName, AsouTypes.String, value);
            }
                break;
            case ContextCallInstructions.CallFunc:
            {
                var funcName = reader.ReadString();
                // TODO: Determine how to handle arguments 
                var parameterName = reader.ReadString();
                throw new NotImplementedException();
            }
            case ContextCallInstructions.ObjectReflection:
            {
                // TODO: Determine how to handle arguments 
                var parameterName = reader.ReadString();
                throw new NotImplementedException();
            }
            case ContextCallInstructions.GetVariableValue:
            {
                var variableName = reader.ReadString();
                var parameterName = reader.ReadString();
                throw new NotImplementedException();
            }
            case ContextCallInstructions.GetProcessParameter:
            {
                var inParameterName = reader.ReadString();
                var outParameterName = reader.ReadString();
                if (processMachineCommands.Parameters.TryGetValue(inParameterName, out var value))
                    processMachineCommands.SetParameter(outParameterName, AsouTypes.UnSet, value);
            }
                break;
            case ContextCallInstructions.GetElementParameter:
            {
                var elementName = reader.ReadString();
                var elementParameterName = reader.ReadString();
                var elementParameterType = reader.ReadType();
                var parameterName = reader.ReadString();
                var value = processMachineCommands.GetElementParameter(elementName, elementParameterName);
                processMachineCommands.SetParameter(parameterName, elementParameterType, value);
            }
                break;
            case ContextCallInstructions.GetElement:
            {
                var elementName = reader.ReadString();
                var parameterName = reader.ReadString();
                if (processMachineCommands.Components.TryGetValue(elementName, out var element))
                    processMachineCommands.SetParameter(parameterName, AsouTypes.Object, element);
            }
                break;
            case ContextCallInstructions.GetCurrentElementParameter:
                throw new NotImplementedException();
            case ContextCallInstructions.CurrentObjectReflection:
                throw new NotImplementedException();
            default:
                throw new ArgumentOutOfRangeException();
        }

        return Task.CompletedTask;
    }
}