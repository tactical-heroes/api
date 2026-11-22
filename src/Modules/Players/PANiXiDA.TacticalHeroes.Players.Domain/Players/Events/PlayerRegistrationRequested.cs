namespace PANiXiDA.TacticalHeroes.Modules.Players.Domain.Players.Events;

public sealed record PlayerRegistrationRequested(Guid UserId) : DomainEvent;
