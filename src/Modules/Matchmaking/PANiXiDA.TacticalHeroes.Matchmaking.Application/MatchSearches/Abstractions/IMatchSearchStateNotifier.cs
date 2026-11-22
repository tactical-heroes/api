using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Common.ReadModels;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Abstractions;

public interface IMatchSearchStateNotifier
{
    Task SendAsync(string connectionId, MatchSearchReadModel state, CancellationToken cancellationToken);
    Task PublishAsync(Guid playerId, MatchSearchReadModel state, CancellationToken cancellationToken);
    Task SubscribeAsync(Guid playerId, string connectionId, CancellationToken cancellationToken);
}
