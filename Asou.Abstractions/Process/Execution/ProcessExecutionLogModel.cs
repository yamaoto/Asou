namespace Asou.Abstractions.Process.Execution;

public record ProcessExecutionLogModel
(
    int Id,
    Guid ProcessContractId,
    Guid ProcessVersionId,
    Guid ProcessInstanceId,
    Guid ThreadId,
    Guid ElementId,
    int State,
    DateTime CreatedOn
);
