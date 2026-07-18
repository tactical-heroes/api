namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.Events;

public sealed record EmailConfirmationRequested(
    Guid UserId,
    string Email,
    string ConfirmationToken,
    DateTimeOffset ExpiresAtUtc) : DomainEvent;
