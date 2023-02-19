namespace Asou.GraphEngine.CodeContractStorage;

public interface IGraphProcessRegistration
{
    bool RegisterFlow(GraphProcessContract graphProcessContract, bool validate = true, bool throwError = true);
    bool ValidateGraph(GraphProcessContract graphProcessContract, bool throwError);
}
