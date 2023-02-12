using Asou.Core.Process.Binding;

namespace Asou.GraphEngine.CodeContractStorage;

public class GraphProcessRegistration : IGraphProcessRegistration
{
    private readonly IParameterDelegateFactory _parameterDelegateFactory;
    private readonly IGraphProcessContractRepository _processContractRepository;

    public GraphProcessRegistration(
        IGraphProcessContractRepository processContractRepository, IParameterDelegateFactory parameterDelegateFactory)
    {
        _processContractRepository = processContractRepository;
        _parameterDelegateFactory = parameterDelegateFactory;
    }

    public void RegisterFlow(GraphProcessContract graphProcessContract)
    {
        // Prepare element parameters getter and setter delegates
        foreach (var (_, node) in graphProcessContract.Nodes)
        {
            foreach (var parameter in node.Parameters)
            {
                _parameterDelegateFactory.CreateDelegates(node.ElementType, parameter.Name);
            }
        }

        _processContractRepository.AddProcessContract(graphProcessContract);
    }
}
