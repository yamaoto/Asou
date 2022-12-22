using Asou.Abstractions.ExecutionElements;

namespace Asou.Core.Process.Binding;

public interface IParameterBinder
{
    void SetParameter<TPropertyType>(BaseElement target, string parameterName, TPropertyType value);
    TPropertyType GetParameter<TPropertyType>(BaseElement target, string parameterName);
}