namespace Asou.Abstractions.Events;

/// <summary>
///     Record representing an event subscription.
/// </summary>
/// <param name="Source">Event source</param>
/// <param name="Type">Event type</param>
/// <param name="Subject">Event Subject</param>
/// <param name="EventSubscriptionType"></param>
public record EventSubscription
(
    string Source,
    string Type,
    string Subject,
    EventSubscriptionType EventSubscriptionType
);
