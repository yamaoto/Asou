using Asou.Abstractions.Events;

namespace Asou.Core;

public record EventSubscriptionModel(
    Guid Id,
    Guid ProcessInstanceId,
    Guid ThreadId,
    string ElementName,
    string Source,
    string Type,
    string Subject,
    bool IsActive,
    EventSubscriptionType EventSubscriptionType
)
{
}