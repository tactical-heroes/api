namespace PANiXiDA.TacticalHeroes.Players.Application.Players.Read.GetInternalPlayer;

public sealed record GetInternalPlayerQuery(Guid UserId) : IQuery<Result<PlayerReadModel>>;
