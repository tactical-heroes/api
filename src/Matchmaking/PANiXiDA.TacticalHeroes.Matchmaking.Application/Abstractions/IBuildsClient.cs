namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.Abstractions;

public interface IBuildsClient
{
    Task<bool> IsOwnedByPlayerAsync(Guid buildId, Guid playerId, CancellationToken cancellationToken);
}
