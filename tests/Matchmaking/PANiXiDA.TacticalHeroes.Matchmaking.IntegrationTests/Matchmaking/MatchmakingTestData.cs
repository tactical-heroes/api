using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Builds;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Matches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Matches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Players;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Matchmaking;

internal static class MatchmakingTestData
{
    internal static MatchmakingTimestamp At(int seconds)
    {
        return MatchmakingTimestamp.Create(
            new DateTimeOffset(2026, 10, 26, 8, 0, 0, TimeSpan.Zero).AddSeconds(seconds));
    }

    internal static MatchSearch CreateSearch(Guid? playerId = null, string mode = "duel")
    {
        return MatchSearch.Create(
            PlayerId.Create(playerId ?? Guid.CreateVersion7()).Value,
            BuildId.Create(Guid.CreateVersion7()).Value,
            RatingSnapshot.Create(1500),
            MatchmakingMode.Create(mode).Value,
            At(0),
            At(300)).Value;
    }

    internal static IUnitOfWork UnitOfWork(IServiceProvider services)
    {
        return services.GetRequiredKeyedService<IUnitOfWork>(typeof(MatchmakingWriteDbContext));
    }

    internal static async Task<Match> SaveMatchAsync(
        IServiceProvider services,
        CancellationToken cancellationToken)
    {
        var first = CreateSearch();
        var second = CreateSearch();
        var match = Match.Create(first, second, At(1), At(60)).Value;
        first.Reserve(match.Id, At(1)).IsSuccess.ShouldBeTrue();
        second.Reserve(match.Id, At(1)).IsSuccess.ShouldBeTrue();
        var searches = services.GetRequiredService<IMatchSearchesRepository>();
        searches.Add(first);
        searches.Add(second);
        services.GetRequiredService<IMatchesRepository>().Add(match);
        await UnitOfWork(services).SaveChangesAsync(cancellationToken);

        return match;
    }
}
