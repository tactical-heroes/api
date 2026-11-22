namespace PANiXiDA.TacticalHeroes.Modules.Identity.Domain.Users.Events;

public sealed record UserRegistered(
    Guid UserId,
    string Email) : DomainEvent;
