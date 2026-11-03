using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchSearches.Watch;

[Authorize]
public sealed class MatchSearchHub(IMediator mediator) : Hub<IMatchSearchClient>
{
    public override async Task OnConnectedAsync()
    {
        var result = await mediator.SendAsync(
            WatchMatchSearchMapper.ToCommand(Context.ConnectionId), Context.ConnectionAborted);
        if (result.IsFailure)
        {
            Context.Abort();
            return;
        }

        await base.OnConnectedAsync();
    }

    internal static string PlayerGroup(Guid playerId) => $"player:{playerId:N}";
}
