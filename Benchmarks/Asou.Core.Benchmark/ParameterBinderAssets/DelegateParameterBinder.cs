using Asou.Abstractions.ExecutionElements;
using Asou.Core.Process.Binding;

namespace Asou.Core.Benchmark.ParameterBinderAssets;

public class DelegateParameterBinder : IParameterDelegateFactory
{
    private readonly Func<string> _getMethod;
    private readonly Action<string> _setMethod;


    public DelegateParameterBinder(TestObject testObject, string parameterName)
    {
        var propertyInfo = testObject.GetType().GetProperty(parameterName)!;
        _getMethod = (Func<string>)Delegate.CreateDelegate(typeof(Func<string>), testObject, propertyInfo.GetMethod!);
        _setMethod =
            (Action<string>)Delegate.CreateDelegate(typeof(Action<string>), testObject, propertyInfo.SetMethod!);
    }

    public void CreateDelegates<TPropertyType>(BaseElement target, string parameterName)
    {
        throw new NotImplementedException();
    }

    public Func<TPropertyType> GetGetParameterDelegate<TPropertyType>(BaseElement target, string parameterName)
    {
        if (target is TestObject && parameterName == "Value") return () => (TPropertyType)(object)_getMethod.Invoke();
        throw new InvalidOperationException();
    }

    public Action<TPropertyType> GetSetParameterDelegate<TPropertyType>(BaseElement target, string parameterName)
    {
        if (target is TestObject && parameterName == "Value") return value => _setMethod.Invoke((string)(object)value!);
        throw new InvalidOperationException();
    }
}