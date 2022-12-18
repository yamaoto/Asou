using Asou.Core.Abstractions.ExecutionElements;

namespace Asou.Core.Process.Binding;

public interface IParameterDelegateFactory
{
    void CreateDelegates<TPropertyType>(BaseElement target, string parameterName);

    Func<TPropertyType> GetGetParameterDelegate<TPropertyType>(BaseElement target, string parameterName);
    Action<TPropertyType> GetSetParameterDelegate<TPropertyType>(BaseElement target, string parameterName);
}