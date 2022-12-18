using Asou.Core.Benchmark.ParameterBinderAssets;
using Asou.Core.Process.Binding;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;

namespace Asou.Core.Benchmark;

#pragma warning disable CS8602
#pragma warning disable CS8604

[MemoryDiagnoser]
[SimpleJob(RunStrategy.Throughput, 1)]
public class ParameterBinderBenchmark
{
    private IParameterBinder? _delegateParameterBinder;
    private ExperimentalSingleBinder? _experimentalSingleBinder;
    private IParameterBinder? _parameterBinder;
    private IParameterBinder? _rawCodeParameterBinder;
    private TestObject? _target;

    [GlobalSetup]
    public void Setup()
    {
        _target = new TestObject();

        var parameterDelegateFactory = new ParameterDelegateFactory();
        parameterDelegateFactory.CreateDelegates<string>(_target, "Value");
        _parameterBinder = new ParameterBinder(parameterDelegateFactory);

        _experimentalSingleBinder = new ExperimentalSingleBinder();
        _experimentalSingleBinder.CreateDelegates<string>(_target, "Value");

        _delegateParameterBinder = new ParameterBinder(new DelegateParameterBinder(_target, "Value"));

        _rawCodeParameterBinder = new ParameterBinder(new RawCodeParameterBinder());
    }

    [Benchmark]
    public string DelegateParameterBinder()
    {
        return Test(_delegateParameterBinder!);
    }

    [Benchmark(Baseline = true)]
    public string ParameterBinder()
    {
        return Test(_parameterBinder!);
    }

    [Benchmark]
    public string ExperimentalSingleBinder()
    {
        return Experiment();
    }

    [Benchmark]
    public string RawCodeParameterBinder()
    {
        return Test(_rawCodeParameterBinder!);
    }

    private string Test(IParameterBinder binder)
    {
        binder.SetParameter(_target, "Value", "Hello");
        var value = binder.GetParameter<string>(_target, "Value");
        binder.SetParameter(_target, "Value", value + " World!");

        var result = binder.GetParameter<string>(_target, "Value");
        if (result != "Hello World!") throw new Exception();

        return result;
    }

    private string Experiment()
    {
        _experimentalSingleBinder.SetParameter(_target, "Value", "Hello");
        var value = _experimentalSingleBinder.GetParameter<string>(_target, "Value");
        _experimentalSingleBinder.SetParameter(_target, "Value", value + " World!");

        var result = _experimentalSingleBinder.GetParameter<string>(_target, "Value");
        if (result != "Hello World!") throw new Exception();

        return result;
    }

    [Benchmark]
    public string ExperimentRead()
    {
        var result = _experimentalSingleBinder.GetParameter<string>(_target, "Value");
        return result;
    }

    [Benchmark]
    public void ExperimentWrite()
    {
        _experimentalSingleBinder.SetParameter(_target, "Value", "Hello World!");
    }
}