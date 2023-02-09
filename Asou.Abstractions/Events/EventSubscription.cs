namespace Asou.Abstractions.Events;

public record EventSubscription
(
    string Source,
    string Type,
    string Subject,
    EventSubscriptionType EventSubscriptionType
)
{
}