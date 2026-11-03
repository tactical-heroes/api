using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.Abstractions;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Watch;

public sealed class WatchMatchSearchHandler(
    IPlayersClient playersClient,
    ICurrentUser currentUser,
    IMatchSearchStateFeed stateFeed)
    : ICommandHandler<WatchMatchSearchCommand, Result<IMatchSearchStateSubscription>>, IRequireAuthorization
{
    public async Task<Result<IMatchSearchStateSubscription>> HandleAsync(
        WatchMatchSearchCommand command,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Result.Failure<IMatchSearchStateSubscription>(Error.Unauthorized("A user account is required."));
        }

        var player = await playersClient.GetByUserIdAsync(userId, cancellationToken);
        if (player is null)
        {
            return Result.Failure<IMatchSearchStateSubscription>(Error.NotFound("Player was not found."));
        }

        return Result.Success(await stateFeed.SubscribeAsync(player.Id, cancellationToken));
    }
}
