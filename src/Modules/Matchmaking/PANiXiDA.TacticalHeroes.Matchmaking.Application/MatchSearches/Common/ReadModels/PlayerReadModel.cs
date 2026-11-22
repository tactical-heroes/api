namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Common.ReadModels;

public sealed record PlayerReadModel(Guid Id, int Rating) : IReadModel;
