namespace PANiXiDA.TacticalHeroes.Modules.Identity.Contracts.Users;

public sealed record UserRegistered(Guid UserId) : IntegrationEvent;
