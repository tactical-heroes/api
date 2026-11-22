namespace PANiXiDA.TacticalHeroes.Players.Application.Players.GetInternalPlayer;

public sealed record GetInternalPlayerQuery(Guid UserId) : IQuery<Result<PlayerReadModel>>;
