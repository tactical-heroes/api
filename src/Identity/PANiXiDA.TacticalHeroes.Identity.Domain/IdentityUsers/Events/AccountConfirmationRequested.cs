namespace PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers.Events;

public sealed record AccountConfirmationRequested(
    Guid UserId,
    string Email,
    string ConfirmationToken) : DomainEvent;
