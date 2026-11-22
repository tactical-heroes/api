using PANiXiDA.TacticalHeroes.Modules.Matchmaking.Domain.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Modules.Matchmaking.Domain.MatchSearches.Specifications;
using PANiXiDA.TacticalHeroes.Modules.Matchmaking.Domain.MatchSearches.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Modules.Matchmaking.Application.MatchSearches.Expire;

public sealed class ExpireMatchSearchesHandler(
    IMatchSearchesRepository matchSearchesRepository,
    TimeProvider timeProvider)
    : ICommandHandler<ExpireMatchSearchesCommand, Result>, IRequireTransaction
{
    public async Task<Result> HandleAsync(ExpireMatchSearchesCommand command, CancellationToken cancellationToken)
    {
        var now = MatchmakingTimestamp.Create(timeProvider.GetUtcNow());

        var searches = await matchSearchesRepository.GetAllAsync(
            new OverdueMatchSearchSpecification(now), cancellationToken);

        foreach (var search in searches)
        {
            var expired = search.Expire(now);
            if (expired.IsFailure)
            {
                return expired;
            }

            matchSearchesRepository.Update(search);
        }

        return Result.Success();
    }
}
