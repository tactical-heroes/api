using PANiXiDA.Core.Application.Messaging.Scheduling;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.ConfirmMatch;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Policies;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Process;

public sealed class ProcessMatchmakingHandler(
    IMatchSearchesReadRepository matchSearchesReadRepository,
    IScheduler scheduler,
    MatchmakingRules rules,
    TimeProvider timeProvider)
    : ICommandHandler<ProcessMatchmakingCommand, Result>, IRequireTransaction
{
    public async Task<Result> HandleAsync(ProcessMatchmakingCommand command, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var searches = await matchSearchesReadRepository.GetSearchingAsync(now, cancellationToken);
        var candidates = searches.Select(search => new MatchmakingCandidate(
            Id: search.Id,
            PlayerId: search.PlayerId,
            Rating: search.Rating,
            Mode: search.Mode,
            QueuedAt: search.QueuedAt,
            ExpiresAt: search.ExpiresAt));
        var pairs = MatchmakingPolicy.SelectPairs(candidates: candidates, now: now, rules: rules);
        foreach (var pair in pairs)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await scheduler.ScheduleAsync(
                command: new ConfirmMatchCommand(pair.FirstSearchId, pair.SecondSearchId),
                delay: TimeSpan.Zero,
                cancellationToken: cancellationToken);
        }

        return Result.Success();
    }
}
