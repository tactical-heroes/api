using PANiXiDA.Core.Application.Messaging.Scheduling;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.ConfirmMatch;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Policies;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Specifications;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Process;

public sealed class ProcessMatchmakingHandler(
    IMatchSearchesRepository matchSearchesRepository,
    IScheduler scheduler,
    MatchmakingRules rules,
    TimeProvider timeProvider)
    : ICommandHandler<ProcessMatchmakingCommand, Result>, IRequireTransaction
{
    public async Task<Result> HandleAsync(ProcessMatchmakingCommand command, CancellationToken cancellationToken)
    {
        var now = MatchmakingTimestamp.Create(timeProvider.GetUtcNow());
        var searches = await matchSearchesRepository.GetAllAsync(
            new SearchingMatchSearchSpecification(now), cancellationToken);
        var candidates = searches.Select(search => new MatchmakingCandidate(
            Id: search.Id.Value,
            PlayerId: search.PlayerId.Value,
            Rating: search.Rating.Value,
            Mode: search.Mode.Name,
            QueuedAt: search.QueuedAt.Value,
            ExpiresAt: search.ExpiresAt.Value));
        var pairs = MatchmakingPolicy.SelectPairs(candidates: candidates, now: now.Value, rules: rules);
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
