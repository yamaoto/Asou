using Asou.Core.Abstractions.ExecutionElements;

namespace Asou.Core.Process.Binding;

public class ParameterBinder : IParameterBinder
{
    private readonly IParameterDelegateFactory _parameterDelegateFactory;

    public ParameterBinder(IParameterDelegateFactory parameterDelegateFactory)
    {
        _parameterDelegateFactory = parameterDelegateFactory;
    }

    public void SetParameter<TPropertyType>(BaseElement target, string parameterName, TPropertyType value)
    {
        var setParameterDelegate =
            _parameterDelegateFactory.GetSetParameterDelegate<TPropertyType>(target, parameterName);
        setParameterDelegate(value);
    }

    public TPropertyType GetParameter<TPropertyType>(BaseElement target, string parameterName)
    {
        var setParameterDelegate =
            _parameterDelegateFactory.GetGetParameterDelegate<TPropertyType>(target, parameterName);
        return setParameterDelegate();
    }
}