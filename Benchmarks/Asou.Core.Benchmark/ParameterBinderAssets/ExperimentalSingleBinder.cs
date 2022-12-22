using System.Runtime.CompilerServices;
using Asou.Abstractions.ExecutionElements;
using Asou.Core.Process.Binding;

namespace Asou.Core.Benchmark.ParameterBinderAssets;

public class ExperimentalSingleBinder : IParameterBinder
{
    private readonly Dictionary<int, Tuple<Delegate, Delegate>> _delegates = new();

    public TPropertyType GetParameter<TPropertyType>(BaseElement target, string parameterName)
    {
        var key = target.ClassName.GetHashCode() * 17 + parameterName.GetHashCode() * 17;

        if (!_delegates.TryGetValue(key, out var tuple)) throw new Exception();

        var method = Unsafe.As<Func<TPropertyType>>(tuple.Item1);
        return method();
    }

    public void SetParameter<TPropertyType>(BaseElement target, string parameterName, TPropertyType value)
    {
        var key = target.ClassName.GetHashCode() * 17 + parameterName.GetHashCode() * 17;

        if (!_delegates.TryGetValue(key, out var tuple)) throw new Exception();

        var method = Unsafe.As<Action<TPropertyType>>(tuple.Item2);
        method(value);
    }

    public void CreateDelegates<TPropertyType>(BaseElement target, string parameterName)
    {
        var propertyInfo = target.GetType().GetProperty(parameterName)!;
        var getMethod =
            (Func<TPropertyType>)Delegate.CreateDelegate(typeof(Func<TPropertyType>), target, propertyInfo.GetMethod!);
        var setMethod =
            (Action<TPropertyType>)Delegate.CreateDelegate(typeof(Action<TPropertyType>), target,
                propertyInfo.SetMethod!);

        var key = target.ClassName.GetHashCode() * 17 + parameterName.GetHashCode() * 17;

        _delegates[key] = new Tuple<Delegate, Delegate>(getMethod, setMethod);
    }
}