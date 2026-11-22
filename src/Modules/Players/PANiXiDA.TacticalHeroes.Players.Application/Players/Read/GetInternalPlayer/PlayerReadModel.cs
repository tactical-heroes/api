namespace PANiXiDA.TacticalHeroes.Players.Application.Players.Read.GetInternalPlayer;

public sealed record PlayerReadModel(Guid Id, int Rating) : IReadModel;
