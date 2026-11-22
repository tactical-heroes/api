using PANiXiDA.TacticalHeroes.Modules.Matchmaking.Application.MatchSearches.Common;

namespace PANiXiDA.TacticalHeroes.Modules.Matchmaking.Application.Abstractions;

public interface IMatchSearchStateNotifier
{
    Task SendAsync(string connectionId, MatchSearchReadModel state, CancellationToken cancellationToken);
    Task PublishAsync(Guid playerId, MatchSearchReadModel state, CancellationToken cancellationToken);
    Task SubscribeAsync(Guid playerId, string connectionId, CancellationToken cancellationToken);
}
