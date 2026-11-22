namespace PANiXiDA.TacticalHeroes.Identity.Contracts.Users;

public sealed record UserRegistered(Guid UserId) : IntegrationEvent;
