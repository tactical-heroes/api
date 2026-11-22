namespace PANiXiDA.TacticalHeroes.Modules.Players.Application.Players.GetInternalPlayer;

public sealed record GetInternalPlayerQuery(Guid UserId) : IQuery<Result<PlayerReadModel>>;
