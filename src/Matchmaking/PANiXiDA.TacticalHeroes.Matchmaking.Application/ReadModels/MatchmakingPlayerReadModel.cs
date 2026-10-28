namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.ReadModels;

public sealed record MatchmakingPlayerReadModel(Guid Id, int Rating) : IReadModel;
