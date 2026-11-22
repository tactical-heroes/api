namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.Events;

public sealed record UserRegistered(
    Guid UserId,
    string Email) : DomainEvent;
