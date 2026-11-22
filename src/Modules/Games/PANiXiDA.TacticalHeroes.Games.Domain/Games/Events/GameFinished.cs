namespace PANiXiDA.TacticalHeroes.Modules.Games.Domain.Games.Events;

public sealed record GameFinished(
    Guid GameId,
    Guid BattleId,
    string Mode,
    Guid? WinnerPlayerId,
    DateTimeOffset FinishedAt) : DomainEvent;
