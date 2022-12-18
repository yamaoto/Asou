using Asou.Core.Process;

namespace Asou.Core.Interpreter;

public class ByteCodeInterpreter
{
    private readonly bool _dryRun;
    private readonly IProcessMachine _processMachine;
    private readonly ByteCodeFormatReader _reader;

    private readonly Dictionary<string, ScriptPointer> _scriptPositions = new();
    private readonly Stack<ReturnPointer> _callStack = new();
#if DEBUG
    public System.Text.StringBuilder DebugOutput = new();
    private string GetDebugOutputTab() => "".PadLeft(_callStack.Count, '\t');
    private void WriteDebugCode(Instructions command, params object[] param) =>
        DebugOutput.AppendLine($"{_reader.Position.ToString("000")}: {GetDebugOutputTab()}{command}({string.Join(" ,", param)})");
#endif


    public ByteCodeInterpreter(
        bool dryRun,
        ByteCodeStorage storage,
        IProcessMachine processMachine
    )
    {
        _reader = new ByteCodeFormatReader(storage.GetCodeForProcess(processMachine.Name));
        _dryRun = dryRun;
        _processMachine = processMachine;
    }

    public async Task<Instructions> EvaluateNextAsync(CancellationToken cancellationToken = default)
    {
        if (_reader.IsEndOfCode) return Instructions.None;
        var read = _reader.ReadByte();
        var instruction = (Instructions)read;

        switch (instruction)
        {
            case Instructions.None:
                return Instructions.None;
            case Instructions.CreateComponent:
            {
                var componentName = _reader.ReadString();
                var name = _reader.ReadString();

#if DEBUG
                WriteDebugCode(instruction, componentName, name);
#endif
                if (_dryRun)
                    break;
                _processMachine.CreateComponent(componentName, name);
            }
                break;
            case Instructions.CreateObject:
                throw new NotImplementedException();
            case Instructions.LetParameter:
            {
                var parameterName = _reader.ReadString();
#if DEBUG
                WriteDebugCode(instruction, parameterName);
#endif
                if (_dryRun)
                    break;
                _processMachine.LetParameter(parameterName);
            }
                break;
            case Instructions.DeleteParameter:
            {
                var parameterName = _reader.ReadString();
#if DEBUG
                WriteDebugCode(instruction, parameterName);
#endif
                if (_dryRun)
                    break;
                _processMachine.DeleteParameter(parameterName);
            }
                break;
            case Instructions.SetParameter:
            {
                var parameterName = _reader.ReadString();
                var parameterType = _reader.ReadType();
                var parameterValue = _reader.ReadValue(parameterType);
#if DEBUG
                WriteDebugCode(instruction, parameterName, parameterType, parameterValue!);
#endif
                if (_dryRun)
                    break;
                _processMachine.SetParameter(parameterName, parameterType, parameterValue);
            }
                break;
            case Instructions.CallProcedure:
            {
                var procedureName = _reader.ReadString();
#if DEBUG
                WriteDebugCode(instruction, procedureName);
#endif
                if (_dryRun)
                    break;
                _processMachine.CallProcedure(procedureName);
            }
                break;
            case Instructions.ExecuteElement:
            {
                var elementName = _reader.ReadString();
#if DEBUG
                WriteDebugCode(instruction, elementName);
#endif
                if (_dryRun)
                    break;
                await _processMachine.ExecuteElementAsync(elementName, cancellationToken);
            }
                break;
            case Instructions.AfterExecuteElement:
            {
                var elementName = _reader.ReadString();
#if DEBUG
                WriteDebugCode(instruction, elementName);
#endif
                if (_dryRun)
                    break;
                await _processMachine.AfterExecuteElementAsync(elementName, cancellationToken);
            }
                break;
            case Instructions.ConfigureAwaiter:
            {
                var elementName = _reader.ReadString();
#if DEBUG
                WriteDebugCode(instruction, elementName);
#endif
                if (_dryRun)
                    break;
                await _processMachine.ConfigureAwaiterAsync(elementName, cancellationToken);
            }
                break;
            case Instructions.ValidateEvent:
                throw new NotImplementedException();
            case Instructions.AfterAwaiter:
                throw new NotImplementedException();
            case Instructions.DispatchEvent:
                throw new NotImplementedException();
            case Instructions.DeclareScript:
            {
                var name = _reader.ReadString();
                var length = _reader.ReadInt32();
                _scriptPositions[name] = new ScriptPointer(_reader.Position, length);
                _reader.Seek(length, SeekOrigin.Current);
#if DEBUG
                WriteDebugCode(instruction, name);
#endif
            }
                break;
            case Instructions.CallScript:
            {
                var name = _reader.ReadString();
                if (!_scriptPositions.ContainsKey(name))
                    throw new Exception($"Script with name '{name}' not registered");
                var end = _scriptPositions[name].Start + _scriptPositions[name].Length;
#if DEBUG
                WriteDebugCode(instruction, name);
#endif
                _callStack.Push(new ReturnPointer(_reader.Position, end));
                _reader.Seek(_scriptPositions[name].Start, SeekOrigin.Begin);
            }
                break;
            case Instructions.Return:
            {
                if (_callStack.TryPeek(out var returnPointer) && _reader.Position == returnPointer.ReadUntil)
                {
#if DEBUG
                    var position = _reader.Position;
                    WriteDebugCode(instruction, position);
#endif
                    _reader.Seek(returnPointer.Position, SeekOrigin.Begin);
                    _callStack.Pop();
                }
            }
                break;
            case Instructions.IfStatement:
                throw new NotImplementedException();
            default:
                throw new ArgumentOutOfRangeException();
        }

        return instruction;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        _reader.Seek(0, SeekOrigin.Begin);
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // TODO: Use ValueTask
            var result = await EvaluateNextAsync(cancellationToken);
            if (result == Instructions.None) break;
        }
    }
}