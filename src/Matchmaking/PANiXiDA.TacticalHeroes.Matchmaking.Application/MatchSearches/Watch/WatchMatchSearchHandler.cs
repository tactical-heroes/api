using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Common;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Watch;

public sealed class WatchMatchSearchHandler(
    IPlayersClient playersClient,
    ICurrentUser currentUser,
    IMatchSearchStateNotifier stateNotifier,
    IMatchSearchesReadRepository matchSearchesReadRepository)
    : ICommandHandler<WatchMatchSearchCommand, Result>, IRequireAuthorization
{
    public async Task<Result> HandleAsync(WatchMatchSearchCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Result.Failure(Error.Unauthorized("A user account is required."));
        }

        var player = await playersClient.GetByUserIdAsync(userId, cancellationToken);
        if (player is null)
        {
            return Result.Failure(Error.NotFound("Player was not found."));
        }

        await stateNotifier.SubscribeAsync(
            playerId: player.Id, connectionId: command.ConnectionId, cancellationToken: cancellationToken);

        var state = await matchSearchesReadRepository.GetStateByPlayerIdAsync(player.Id, cancellationToken)
            ?? new MatchSearchReadModel(
                Id: null, Status: null, GameId: null, QueuedAt: null, ExpiresAt: null);

        await stateNotifier.SendAsync(
            connectionId: command.ConnectionId, state: state, cancellationToken: cancellationToken);

        return Result.Success();
    }
}
