namespace Asou.Core.Process.Binding;

public interface IParameterDelegateFactory
{
    void CreateDelegates<TElement, TPropertyType>(string parameterName);
    void CreateDelegates(Type elementType, string parameterName);
    Func<object, TPropertyType> GetParameterGetter<TPropertyType>(Type elementType, string parameterName);
    Action<object, TPropertyType> GetParameterSetter<TPropertyType>(Type elementType, string parameterName);


    void SetParameter(Type elementType, object target, string parameterName, object value);
    object GetParameter(Type elementType, object target, string parameterName);
}
