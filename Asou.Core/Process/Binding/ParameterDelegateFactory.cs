using System.Runtime.CompilerServices;

namespace Asou.Core.Process.Binding;

public class ParameterDelegateFactory : IParameterDelegateFactory
{
    private readonly Dictionary<int, Tuple<Delegate, Delegate>> _delegates = new();

    public Func<object, TPropertyType> GetParameterGetter<TPropertyType>(Type elementType, string parameterName)
    {
        var key = elementType.Name.GetHashCode() * 17 + parameterName.GetHashCode() * 17;
        if (!_delegates.TryGetValue(key, out var value))
        {
            throw new Exception("Delegate not found");
        }

        return Unsafe.As<Func<object, TPropertyType>>(value.Item1);
    }

    public Action<object, TPropertyType> GetParameterSetter<TPropertyType>(Type elementType, string parameterName)
    {
        var key = elementType.Name.GetHashCode() * 17 + parameterName.GetHashCode() * 17;
        if (!_delegates.TryGetValue(key, out var value))
        {
            throw new Exception("Delegate not found");
        }

        return Unsafe.As<Action<object, TPropertyType>>(value.Item2);
    }

    public void SetParameter(Type elementType, object target, string parameterName, object value)
    {
        var key = elementType.Name.GetHashCode() * 17 + parameterName.GetHashCode() * 17;
        if (!_delegates.TryGetValue(key, out var pair))
        {
            throw new Exception("Delegate not found");
        }

        Unsafe.As<Action<object, object>>(pair.Item2).Invoke(target, value);
    }

    public object GetParameter(Type elementType, object target, string parameterName)
    {
        var key = elementType.Name.GetHashCode() * 17 + parameterName.GetHashCode() * 17;
        if (!_delegates.TryGetValue(key, out var pair))
        {
            throw new Exception("Delegate not found");
        }

        return Unsafe.As<Func<object, object>>(pair.Item1).Invoke(target);
    }

    public void CreateDelegates<TElement, TPropertyType>(string parameterName)
    {
        var propertyInfo = typeof(TElement).GetProperty(parameterName)!;
        var getMethod =
            (Func<TElement, TPropertyType>)Delegate.CreateDelegate(typeof(Func<TElement, TPropertyType>),
                propertyInfo.GetMethod!);
        var setMethod =
            (Action<TElement, TPropertyType>)Delegate.CreateDelegate(typeof(Action<TElement, TPropertyType>),
                propertyInfo.SetMethod!);

        var key = typeof(TElement).Name.GetHashCode() * 17 + parameterName.GetHashCode() * 17;
        _delegates[key] = new Tuple<Delegate, Delegate>(getMethod, setMethod);
    }

    public void CreateDelegates(Type elementType, string parameterName)
    {
        var propertyInfo = elementType.GetProperty(parameterName)!;

        var getterType = typeof(Func<,>).MakeGenericType(elementType, propertyInfo.PropertyType);
        var setterType = typeof(Action<,>).MakeGenericType(elementType, propertyInfo.PropertyType);

        var getMethod = Delegate.CreateDelegate(getterType, propertyInfo.GetMethod!);
        var setMethod = Delegate.CreateDelegate(setterType, propertyInfo.SetMethod!);

        var key = elementType.Name.GetHashCode() * 17 + parameterName.GetHashCode() * 17;
        _delegates[key] = new Tuple<Delegate, Delegate>(getMethod, setMethod);
    }
}
