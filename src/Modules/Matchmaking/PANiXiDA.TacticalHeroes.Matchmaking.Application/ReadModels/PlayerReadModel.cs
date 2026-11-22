namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.ReadModels;

public sealed record PlayerReadModel(Guid Id, int Rating) : IReadModel;
