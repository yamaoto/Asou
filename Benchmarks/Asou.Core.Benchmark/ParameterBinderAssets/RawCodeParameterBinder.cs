using Asou.Core.Abstractions.ExecutionElements;
using Asou.Core.Process.Binding;

namespace Asou.Core.Benchmark.ParameterBinderAssets;

public class RawCodeParameterBinder : IParameterDelegateFactory
{
    public void CreateDelegates<TPropertyType>(BaseElement target, string parameterName)
    {
        throw new NotImplementedException();
    }

    public Func<TPropertyType> GetGetParameterDelegate<TPropertyType>(BaseElement target, string parameterName)
    {
        if (target is TestObject testObject)
            return parameterName switch
            {
                "Value" => () => (TPropertyType)(object)testObject.Value,
                _ =>
                    throw new InvalidOperationException()
            };

        throw new InvalidOperationException();
    }

    public Action<TPropertyType> GetSetParameterDelegate<TPropertyType>(BaseElement target, string parameterName)
    {
        if (target is TestObject testObject)
            return parameterName switch
            {
                "Value" => value => testObject.Value = (string)(object)value!,
                _ =>
                    throw new InvalidOperationException()
            };

        throw new InvalidOperationException();
    }
}