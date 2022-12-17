using Asou.Core.Benchmark.ProcessMachineAssets;
using BenchmarkDotNet.Attributes;

namespace Asou.Core.Benchmark;

[MemoryDiagnoser]
public class ProcessMachineMemoryAllocations
{
    private ByteCodeInterpreter? _byteCodeInterpreter;
    private ProcessMachine? _processMachine;

    [Params(10, 100, 1000)] public int Count { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _processMachine = new ProcessMachine
        {
            ComponentFactory = (name, objectName) => null!,
            Name = nameof(ProcessMachineBenchmark)
        };
        _byteCodeInterpreter = new ByteCodeInterpreter(true, BenchmarkHelper.GetTestCode(Count), _processMachine);
    }

    [Benchmark(OperationsPerInvoke = 1)]
    public async Task Test()
    {
        await _byteCodeInterpreter!.RunAsync();
    }
}