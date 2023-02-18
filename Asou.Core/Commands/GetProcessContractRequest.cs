using Asou.Abstractions.Process.Contract;

namespace Asou.Core.Commands;

public class GetProcessContractRequest
{
    private readonly IProcessContractRepository _processContractRepository;

    public GetProcessContractRequest(IProcessContractRepository processContractRepository)
    {
        _processContractRepository = processContractRepository;
    }

    public async Task<ProcessContract?> RequestAsync(Guid payload, CancellationToken cancellationToken = default)
    {
        var processContract = await _processContractRepository.GetActiveProcessContractAsync(payload);
        return processContract;
    }
}
