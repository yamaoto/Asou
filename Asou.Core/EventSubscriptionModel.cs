using Asou.Abstractions.Events;

namespace Asou.Core;

public record EventSubscriptionModel(
    Guid Id,
    Guid ProcessInstanceId,
    Guid ThreadId,
    Guid ElementId,
    string Source,
    string Type,
    string Subject,
    EventSubscriptionType EventSubscriptionType
)
{
    public bool IsActive { get; set; } = true;
}
