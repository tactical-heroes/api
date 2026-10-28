namespace PANiXiDA.TacticalHeroes.Players.Application.Players.GetProfileByUserId;

public sealed record PlayerProfileReadModel(Guid Id, int Rating) : IReadModel;
