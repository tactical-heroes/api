using PANiXiDA.TacticalHeroes.Modules.Matchmaking.Presentation.Features.MatchSearches.Streaming.State;

namespace PANiXiDA.TacticalHeroes.Modules.Matchmaking.Presentation.Features.MatchSearches.Watch;

public interface IMatchSearchClient
{
    /// <summary>Sends the current match search state to the connected player.</summary>
    Task StateChangedAsync(MatchSearchStateResponse response, CancellationToken cancellationToken);
}
