using Asou.Abstractions;

namespace Asou.Core.Interpreter.Extensions;

public class ContextCallInterpreter : IByteCodeInterpreterExtension
{
    public const byte ContextCallExtensionCode = 1;

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
                processMachineCommands.SetParameter(parameterName, AsouTypes.UnSet, configPath);
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
                var value = GetElementParameter(processMachineCommands, elementParameterType, elementName,
                    elementParameterName);
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

    private object? GetElementParameter(IProcessMachineCommands processMachineCommands, AsouTypes elementParameterType,
        string elementName, string elementParameterName)
    {
        return elementParameterType switch
        {
            AsouTypes.UnSet => throw new NotImplementedException(),
            AsouTypes.Boolean => processMachineCommands.GetElementParameter<bool?>(elementName, elementParameterName),
            AsouTypes.Integer => processMachineCommands.GetElementParameter<int?>(elementName, elementParameterName),
            AsouTypes.Float => processMachineCommands.GetElementParameter<float?>(elementName, elementParameterName),
            AsouTypes.Decimal => processMachineCommands.GetElementParameter<decimal?>(elementName,
                elementParameterName),
            AsouTypes.String => processMachineCommands.GetElementParameter<string?>(elementName, elementParameterName),
            AsouTypes.DateTime => processMachineCommands.GetElementParameter<DateTime?>(elementName,
                elementParameterName),
            AsouTypes.Guid => processMachineCommands.GetElementParameter<Guid?>(elementName, elementParameterName),
            AsouTypes.Object => throw new NotImplementedException(),
            AsouTypes.ObjectLink => throw new NotImplementedException(),
            AsouTypes.NullObject => null,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}