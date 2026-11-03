namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.Abstractions;

public interface IMatchSearchStateFeed
{
    Task<IMatchSearchStateSubscription> SubscribeAsync(Guid playerId, CancellationToken cancellationToken);
}
