using Microsoft.AspNetCore.SignalR;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchSearches.Watch;

public interface IMatchSearchClient
{
    [HubMethodName("StateChanged")]
    Task StateChangedAsync(MatchSearchStateResponse response, CancellationToken cancellationToken);
}
