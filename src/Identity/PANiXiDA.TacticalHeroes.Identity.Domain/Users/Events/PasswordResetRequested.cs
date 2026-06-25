namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.Events;

public sealed record PasswordResetRequested(
    Guid UserId,
    string Email,
    string PasswordResetToken) : DomainEvent;
