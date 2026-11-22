namespace PANiXiDA.TacticalHeroes.Players.Application.Players.GetInternalPlayer;

public sealed record PlayerReadModel(Guid Id, int Rating) : IReadModel;
