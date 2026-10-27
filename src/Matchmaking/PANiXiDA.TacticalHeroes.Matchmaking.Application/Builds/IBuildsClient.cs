namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.Builds;

public interface IBuildsClient
{
    Task<bool> IsOwnedByPlayerAsync(Guid buildId, Guid playerId, CancellationToken cancellationToken);
}
