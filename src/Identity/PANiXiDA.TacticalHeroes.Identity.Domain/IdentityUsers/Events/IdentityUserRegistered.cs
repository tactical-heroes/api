namespace PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers.Events;

public sealed record IdentityUserRegistered(
    Guid UserId,
    string Email) : DomainEvent;
