namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.Matches.Events;

public sealed record GameCreationRequested(Guid MatchId) : DomainEvent;
