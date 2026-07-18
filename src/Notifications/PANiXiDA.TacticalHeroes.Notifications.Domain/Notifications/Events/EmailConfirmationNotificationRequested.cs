namespace PANiXiDA.TacticalHeroes.Notifications.Domain.Notifications.Events;

public sealed record EmailConfirmationNotificationRequested(
    Guid IntegrationEventId,
    Guid UserId,
    string Email,
    string ConfirmationUrl,
    DateTimeOffset ExpiresAtUtc) : DomainEvent;
