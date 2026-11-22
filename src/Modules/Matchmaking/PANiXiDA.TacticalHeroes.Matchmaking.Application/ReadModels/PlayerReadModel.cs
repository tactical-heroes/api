namespace PANiXiDA.TacticalHeroes.Modules.Matchmaking.Application.ReadModels;

public sealed record PlayerReadModel(Guid Id, int Rating) : IReadModel;
