using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.DependencyInjection;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Testing.Databases;

namespace PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Infrastructure.Streaming;

[Collection(IntegrationTestCollectionDefinition.Name)]
public sealed class MatchSearchStateMigrationTests
{
    [Fact(DisplayName = "State stream should preserve active search when backfilling state versions")]
    public async Task MigrateAsync_Should_PreserveActiveSearch_When_BackfillingStateVersions()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var database = new PostgreSqlTestDatabase("matchmaking", isolateDatabase: true);
        await database.InitializeAsync(ct);
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:PostgreSqlConnectionString"] = database.PostgreSqlConnectionString
            })
            .Build();
        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<MatchmakingWriteDbContext>();
        await context.GetService<IMigrator>().MigrateAsync("20261027182154_AddMatchSearchGameId", ct);
        var playerId = Guid.NewGuid();
        var activeId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO matchmaking.match_searches
                (id, player_id, build_id, rating, mode, status, queued_at, expires_at, created_at, updated_at)
            VALUES ({activeId}, {playerId}, {Guid.NewGuid()}, 1500, 'RankedDuel', 'Searching',
                {now}, {now.AddMinutes(5)}, {now}, {now});
            INSERT INTO matchmaking.match_searches
                (id, player_id, build_id, rating, mode, status, queued_at, expires_at, created_at, updated_at)
            VALUES ({otherId}, {playerId}, {Guid.NewGuid()}, 1500, 'RankedDuel', 'Cancelled',
                {now.AddHours(1)}, {now.AddHours(1).AddMinutes(5)}, {now}, {now});
            """,
            ct);

        await context.Database.MigrateAsync(ct);

        var state = await scope.ServiceProvider.GetRequiredService<IMatchSearchesReadRepository>()
            .GetStateByPlayerIdAsync(playerId, ct);
        state.ShouldNotBeNull().MatchSearchId.ShouldBe(activeId);
        state.Status.ShouldBe("Searching");
        state.Version.ShouldBeGreaterThan(0);
        context.Database.HasPendingModelChanges().ShouldBeFalse();
    }
}
