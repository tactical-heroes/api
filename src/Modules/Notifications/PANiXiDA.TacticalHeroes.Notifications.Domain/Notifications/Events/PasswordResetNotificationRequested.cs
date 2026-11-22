namespace PANiXiDA.TacticalHeroes.Notifications.Domain.Notifications.Events;

public sealed record PasswordResetNotificationRequested(
    Guid IntegrationEventId,
    Guid UserId,
    string Email,
    string PasswordResetUrl,
    DateTimeOffset ExpiresAtUtc) : DomainEvent;
