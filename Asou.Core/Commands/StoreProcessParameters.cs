using Asou.Abstractions.Container;

namespace Asou.Core.Commands;

public record struct StoreProcessParameters(Guid ProcessInstanceId, Dictionary<string, ValueContainer> Parameters);
