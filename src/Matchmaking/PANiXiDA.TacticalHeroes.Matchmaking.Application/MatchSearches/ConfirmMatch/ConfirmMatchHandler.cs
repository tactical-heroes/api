using PANiXiDA.Core.Application.Messaging.EventBus;

using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Events;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Policies;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.ConfirmMatch;

public sealed class ConfirmMatchHandler(
    IMatchSearchesRepository matchSearchesRepository,
    IEventBus eventBus,
    TimeProvider timeProvider)
    : ICommandHandler<ConfirmMatchCommand, Result>, IRequireTransaction
{
    public async Task<Result> HandleAsync(ConfirmMatchCommand command, CancellationToken cancellationToken)
    {
        var firstId = MatchSearchId.Create(command.FirstSearchId);
        var secondId = MatchSearchId.Create(command.SecondSearchId);
        var validation = Result.Combine(firstId, secondId);
        if (validation.IsFailure)
        {
            return Result.Failure(validation.Errors);
        }

        var first = await matchSearchesRepository.GetByIdAsync(firstId.Value, cancellationToken);
        var second = await matchSearchesRepository.GetByIdAsync(secondId.Value, cancellationToken);
        if (first is null
            || second is null
            || first.Status != MatchSearchStatus.Searching
            || second.Status != MatchSearchStatus.Searching)
        {
            return Result.Success();
        }

        var now = MatchmakingTimestamp.Create(timeProvider.GetUtcNow());

        if (!MatchmakingPolicy.CanMatch(first: first, second: second, now: now))
        {
            return Result.Success();
        }

        var matched = Result.Combine(first.Matched(now), second.Matched(now));

        if (matched.IsFailure)
        {
            return matched;
        }

        matchSearchesRepository.Update(first);
        matchSearchesRepository.Update(second);

        await eventBus.PublishAsync(
            @event: new MatchFound(
                FirstMatchSearchId: first.Id.Value,
                FirstPlayerId: first.PlayerId.Value,
                FirstBuildId: first.BuildId.Value,
                FirstRating: first.Rating.Value,
                SecondMatchSearchId: second.Id.Value,
                SecondPlayerId: second.PlayerId.Value,
                SecondBuildId: second.BuildId.Value,
                SecondRating: second.Rating.Value,
                Mode: first.Mode.Name,
                MatchedAt: now.Value),
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}
