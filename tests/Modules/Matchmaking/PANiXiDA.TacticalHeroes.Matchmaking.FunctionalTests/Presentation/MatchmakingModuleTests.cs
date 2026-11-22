using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation;

[Collection(FunctionalTestCollectionDefinition.Name)]
public sealed class MatchmakingModuleTests(FunctionalTestFixture fixture)
{
    [Fact(DisplayName = "Host should provide migrated Matchmaking contexts when started")]
    public async Task Host_Should_ProvideMigratedMatchmakingContexts_When_Started()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = fixture.Services.CreateAsyncScope();
        var writeContext = scope.ServiceProvider.GetRequiredService<MatchmakingWriteDbContext>();
        var readContext = scope.ServiceProvider.GetRequiredService<MatchmakingReadDbContext>();

        var appliedMigrations = await writeContext.Database.GetAppliedMigrationsAsync(cancellationToken);
        var canRead = await readContext.Database.CanConnectAsync(cancellationToken);

        appliedMigrations.ShouldNotBeEmpty();
        canRead.ShouldBeTrue();
    }
}
