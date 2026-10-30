namespace PANiXiDA.TacticalHeroes.Games.Domain.Games.Events;

public sealed record GameCreated(
    Guid GameId,
    Guid FirstMatchSearchId,
    Guid SecondMatchSearchId) : DomainEvent;
