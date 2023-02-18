using Asou.Abstractions.Container;
using Asou.Abstractions.Process.Contract;
using Asou.Abstractions.Process.Instance;

namespace Asou.Core.Commands;

public record struct ResumeProcessRecord(ProcessInstanceModel ProcessInstance, ProcessContract? ProcessContract,
    Dictionary<string, ValueContainer> Parameters);
