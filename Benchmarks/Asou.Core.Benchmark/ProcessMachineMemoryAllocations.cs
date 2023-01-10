using Asou.ByteCodeEngine;
using Asou.Core.Benchmark.ProcessMachineAssets;
using Asou.Core.Process;
using Asou.Core.Process.Binding;
using BenchmarkDotNet.Attributes;

namespace Asou.Core.Benchmark;

[MemoryDiagnoser]
public class ProcessMachineMemoryAllocations
{
    private ByteCodeInterpreter? _byteCodeInterpreter;
    private ProcessRuntime? _processMachine;

    [Params(10, 100, 1000)] public int Count { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _processMachine = new ProcessRuntime(new ParameterDelegateFactory(),
            nameof(ProcessMachineBenchmark)) { ComponentFactory = (name, objectName) => null! };
        _byteCodeInterpreter =
            new ByteCodeInterpreter(true, BenchmarkHelper.GetTestCode(Count), _processMachine);
    }

    [Benchmark(OperationsPerInvoke = 1)]
    public async Task Test()
    {
        await _byteCodeInterpreter!.RunAsync();
    }
}