using Asou.Core.Benchmark.ProcessMachineAssets;
using BenchmarkDotNet.Attributes;

namespace Asou.Core.Benchmark;

[MinColumn]
[MaxColumn]
[MeanColumn]
[MedianColumn]
public class ProcessMachineBenchmark
{
    private ByteCodeInterpreter? _byteCodeInterpreter;
    private ProcessMachine? _processMachine;

#if DEBUG
    public static string GetCode(ProcessMachineBenchmark benchmark) => benchmark._byteCodeInterpreter!.DebugOutput.ToString();
#endif

    [GlobalSetup]
    public void GlobalSetup()
    {
        _processMachine = new ProcessMachine
        {
            ComponentFactory = (name, objectName) => CreateElement(name),
            Name = nameof(ProcessMachineBenchmark)
        };
        _byteCodeInterpreter = new ByteCodeInterpreter(false, BenchmarkHelper.GetTestCode(Count), _processMachine);
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