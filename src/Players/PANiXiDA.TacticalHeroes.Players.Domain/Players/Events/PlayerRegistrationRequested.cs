namespace PANiXiDA.TacticalHeroes.Players.Domain.Players.Events;

public sealed record PlayerRegistrationRequested(
    Guid IntegrationEventId,
    Guid UserId) : DomainEvent;
