using Asou.Abstractions;
using Microsoft.Extensions.Logging;

namespace Asou.Core;

public class ProcessExecutionEngine
{
    private readonly IProcessExecutionDriver _driver;
    private readonly ILogger<ProcessExecutionEngine> _logger;
    private readonly IProcessContractRepository _processContractRepository;
    private readonly IProcessInstanceRepository _processInstanceRepository;

    public ProcessExecutionEngine(
        IProcessExecutionDriver driver,
        IProcessContractRepository processContractRepository,
        IProcessInstanceRepository processInstanceRepository,
        ILogger<ProcessExecutionEngine> logger
    )
    {
        _driver = driver;
        _processContractRepository = processContractRepository;
        _processInstanceRepository = processInstanceRepository;
        _logger = logger;
    }

    // TODO: Resume execution after restart
    // TODO: Handle events: send events to process
    // TODO: Handle events: resume awaiter

    public async Task<ProcessParameters> ExecuteAsync(Guid processContractId, ProcessParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var processContract =
            await _processContractRepository.GetActiveProcessContractAsync(processContractId);
        if (processContract == null) throw new Exception("processContract not exists");

        return await ExecuteAsync(processContract, parameters, cancellationToken);
    }

    public async Task<ProcessParameters> ExecuteAsync(Guid processContractId, Guid processVersionId, int versionNumber,
        ProcessParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var processContract =
            await _processContractRepository.GetProcessContractAsync(processContractId, processVersionId,
                versionNumber);
        if (processContract == null) throw new Exception("processContract not exists");

        return await ExecuteAsync(processContract, parameters, cancellationToken);
    }

    private async Task<ProcessParameters> ExecuteAsync(ProcessContract processContract,
        ProcessParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var instance = await _driver.CreateInstanceAsync(processContract, cancellationToken);
        var state = ProcessInstanceGlobalStates.Created;
        if (instance.PersistType != PersistType.No)
            await _processInstanceRepository.CreateInstance(instance.Id,
                instance.ProcessContract.ProcessContractId,
                instance.ProcessContract.ProcessVersionId, instance.ProcessContract.VersionNumber,
                state);

        foreach (var (parameter, value) in parameters)
            instance.ProcessRuntime.SetParameter(parameter, AsouTypes.UnSet, value);

        try
        {
            state = ProcessInstanceGlobalStates.Running;
            if (instance.PersistType != PersistType.No)
                await _processInstanceRepository.UpdateInstance(instance.Id,
                    instance.ProcessContract.ProcessContractId,
                    instance.ProcessContract.ProcessVersionId, instance.ProcessContract.VersionNumber,
                    state);

            var result = await _driver.RunAsync(instance, cancellationToken);
            state = ProcessInstanceGlobalStates.Finished;
            return result;
        }
        catch (Exception e)
        {
            state = ProcessInstanceGlobalStates.Error;
            // TODO: Provide error information to process instance
            _logger.LogError(e, "Process {ProcessContractName} ID {ProcessInstanceId} thrown error",
                instance.ProcessContract.Name, instance.Id);
            throw;
        }
        finally
        {
            if (instance.PersistType != PersistType.No)
                await _processInstanceRepository.UpdateInstance(instance.Id,
                    instance.ProcessContract.ProcessContractId,
                    instance.ProcessContract.ProcessVersionId, instance.ProcessContract.VersionNumber,
                    state);
        }
    }
}