namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.Events;

public sealed record AccountConfirmationRequested(
    Guid UserId,
    string Email,
    string ConfirmationToken) : DomainEvent;
