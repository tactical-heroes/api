namespace PANiXiDA.TacticalHeroes.Modules.Identity.Domain.Users.Events;

public sealed record PasswordResetRequested(
    Guid UserId,
    string Email,
    string PasswordResetToken,
    DateTimeOffset ExpiresAtUtc) : DomainEvent;
