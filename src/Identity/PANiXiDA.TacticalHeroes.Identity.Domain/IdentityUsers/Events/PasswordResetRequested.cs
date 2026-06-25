namespace PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers.Events;

public sealed record PasswordResetRequested(
    Guid UserId,
    string Email,
    string PasswordResetToken) : DomainEvent;
