using Asou.Abstractions.Events;

namespace Asou.Core;

public record EventSubscriptionModel(
    Guid Id,
    Guid processInstanceId,
    string ElementName,
    string Source,
    string Type,
    string Subject,
    bool IsActive,
    EventSubscriptionType EventSubscriptionType
)
{
}