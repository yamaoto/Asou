using Asou.Abstractions.ExecutionElements;
using Asou.ByteCodeEngine;
using Asou.Core.Benchmark.ProcessMachineAssets;
using Asou.Core.Process;
using Asou.Core.Process.Binding;
using BenchmarkDotNet.Attributes;

namespace Asou.Core.Benchmark;

[MinColumn]
[MaxColumn]
[MeanColumn]
[MedianColumn]
public class ProcessMachineBenchmark
{
    private ByteCodeInterpreter? _byteCodeInterpreter;
    private ProcessRuntime? _processMachine;

#if DEBUG
    public static string GetCode(ProcessMachineBenchmark benchmark) => benchmark._byteCodeInterpreter!.DebugOutput.ToString();
#endif

    [GlobalSetup]
    public void GlobalSetup()
    {
        _processMachine = new ProcessRuntime(new ParameterDelegateFactory(),
            nameof(ProcessMachineBenchmark)) { ComponentFactory = (name, objectName) => CreateElement(name) };
        _byteCodeInterpreter =
            new ByteCodeInterpreter(false, BenchmarkHelper.GetTestCode(Count), _processMachine);
    }

    [Params(10, 100, 1000)] public int Count { get; set; }

    private BaseElement CreateElement(string name)
    {
        return name switch
        {
            "start" => new Start(),
            "end" => new End(),
            _ => new DoWork()
        };
    }

    [Benchmark(OperationsPerInvoke = 1)]
    public async Task Test()
    {
        await _byteCodeInterpreter!.RunAsync();
    }
}