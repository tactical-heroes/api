using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Persistence;

using PANiXiDA.TacticalHeroes.Matchmaking.Contracts.MatchSearches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Builds;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Players;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchSearches.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation.Matching;

internal static class MatchmakingTestSupport
{
    internal static MatchSearch CreateSearch(
        int rating = 1500, string mode = "RankedDuel", DateTimeOffset? queuedAt = null)
    {
        return MatchSearch.Create(
            PlayerId.New(),
            BuildId.New(),
            Rating.Create(rating),
            MatchmakingMode.Create(mode).Value,
            MatchmakingTimestamp.Create(queuedAt ?? DateTimeOffset.UtcNow.AddSeconds(-30))).Value;
    }

    internal static async Task SeedAsync(
        IServiceProvider services, IEnumerable<MatchSearch> searches, CancellationToken ct)
    {
        await using var scope = services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>();
        foreach (var search in searches)
        {
            repository.Add(search);
        }

        await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(MatchmakingWriteDbContext))
            .SaveChangesAsync(ct);
    }

    internal static async Task<IReadOnlyList<MatchSearchReadDbModel>> ReadAsync(
        IServiceProvider services, Guid[] ids, CancellationToken ct)
    {
        await using var scope = services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<MatchmakingReadDbContext>()
            .Set<MatchSearchReadDbModel>()
            .Where(search => ids.Contains(search.Id))
            .ToListAsync(ct);
    }

    internal static async Task<MatchFound> WaitForFoundAsync(
        IServiceProvider services, Guid searchId, CancellationToken ct)
    {
        var probe = services.GetRequiredService<MatchFoundProbe>();
        var deadline = DateTimeOffset.UtcNow.AddSeconds(15);
        while (DateTimeOffset.UtcNow < deadline)
        {
            var found = probe.Events.FirstOrDefault(found =>
                found.FirstMatchSearchId == searchId || found.SecondMatchSearchId == searchId);
            if (found is not null)
            {
                return found;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(50), ct);
        }

        throw new TimeoutException($"MatchFound was not delivered for search {searchId}.");
    }
}
