using PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchSearches.Streaming.State;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchSearches.Watch;

public interface IMatchSearchClient
{
    Task StateChangedAsync(MatchSearchStateResponse response, CancellationToken cancellationToken);
}
