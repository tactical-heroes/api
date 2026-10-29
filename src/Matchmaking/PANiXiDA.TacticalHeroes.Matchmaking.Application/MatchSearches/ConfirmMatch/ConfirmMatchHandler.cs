using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Policies;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.ConfirmMatch;

public sealed class ConfirmMatchHandler(
    IMatchSearchesRepository matchSearchesRepository,
    MatchmakingRules rules,
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
        if (!MatchmakingPolicy.CanMatch(
            first: ToCandidate(first), second: ToCandidate(second), now: now.Value, rules: rules))
        {
            return Result.Success();
        }

        var matched = first.MatchWith(second, now);
        if (matched.IsFailure)
        {
            return matched;
        }

        matchSearchesRepository.Update(first);
        matchSearchesRepository.Update(second);

        return Result.Success();
    }

    private static MatchmakingCandidate ToCandidate(MatchSearch search)
    {
        return new MatchmakingCandidate(
            Id: search.Id.Value,
            PlayerId: search.PlayerId.Value,
            Rating: search.Rating.Value,
            Mode: search.Mode.Name,
            QueuedAt: search.QueuedAt.Value,
            ExpiresAt: search.ExpiresAt.Value);
    }
}
