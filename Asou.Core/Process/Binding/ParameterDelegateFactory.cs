using Asou.Abstractions.ExecutionElements;

namespace Asou.Core.Process.Binding;

public class ParameterDelegateFactory : IParameterDelegateFactory
{
    private readonly Dictionary<int, Tuple<Delegate, Delegate>> _delegates = new();

    public Func<TPropertyType> GetGetParameterDelegate<TPropertyType>(BaseElement target, string parameterName)
    {
        var key = target.ClassName.GetHashCode() * 17 + parameterName.GetHashCode() * 17;
        if (!_delegates.TryGetValue(key, out var value)) CreateDelegates<TPropertyType>(target, parameterName);

        return (Func<TPropertyType>)value!.Item1;
    }

    public Action<TPropertyType> GetSetParameterDelegate<TPropertyType>(BaseElement target, string parameterName)
    {
        var key = target.ClassName.GetHashCode() * 17 + parameterName.GetHashCode() * 17;
        if (!_delegates.TryGetValue(key, out var value)) CreateDelegates<TPropertyType>(target, parameterName);

        return (Action<TPropertyType>)value!.Item2;
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