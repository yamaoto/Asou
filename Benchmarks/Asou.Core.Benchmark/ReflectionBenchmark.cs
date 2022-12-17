using System.Reflection;
using Asou.Core.Benchmark.ReflectionAssets;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;

namespace Asou.Core.Benchmark;

[MemoryDiagnoser]
[SimpleJob(RunStrategy.Throughput, 1)]
public class ReflectionBenchmark
{
    private Func<int>? _getMethod;
    private PropertyInfo? _propertyInfo;
    private Action<int>? _setMethod;
    private TestObject? _target;

    [GlobalSetup]
    public void Setup()
    {
        _target = new TestObject();
        _propertyInfo = typeof(TestObject).GetProperty("Value")!;
        _getMethod = (Func<int>)Delegate.CreateDelegate(typeof(Func<int>), _target, _propertyInfo.GetMethod!);
        _setMethod = (Action<int>)Delegate.CreateDelegate(typeof(Action<int>), _target, _propertyInfo.SetMethod!);
    }

    [Benchmark(Baseline = true)]
    public int Simple()
    {
        _target!.Value = int.MinValue;
        var value = _target.Value;
        _target.Value = value + 1;
        return _target.Value;
    }

    [Benchmark]
    public int Reflection()
    {
        var property = typeof(TestObject).GetProperty("Value")!;
        property.SetValue(_target, int.MinValue);
        var value = (int)property.GetValue(_target)!;
        property.SetValue(_target, value + 1);
        return (int)property.GetValue(_target)!;
    }

    [Benchmark]
    public int ReflectionCached()
    {
        _propertyInfo!.SetValue(_target, int.MinValue);
        var value = (int)_propertyInfo.GetValue(_target)!;
        _propertyInfo.SetValue(_target, value + 1);
        return (int)_propertyInfo.GetValue(_target)!;
    }

    [Benchmark]
    public int ReflectionDelegateCached()
    {
        _setMethod!.Invoke(int.MinValue);
        var value = _getMethod!.Invoke();
        _setMethod(value + 1);
        return _getMethod();
    }
}