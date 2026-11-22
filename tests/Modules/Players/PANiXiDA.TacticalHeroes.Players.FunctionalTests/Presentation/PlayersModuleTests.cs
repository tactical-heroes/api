using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Players.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Players.FunctionalTests.Presentation;

[Collection(FunctionalTestCollectionDefinition.Name)]
public sealed class PlayersModuleTests(FunctionalTestFixture fixture)
{
    [Fact(DisplayName = "Host should provide migrated Players contexts when started")]
    public async Task Host_Should_ProvideMigratedPlayersContexts_When_Started()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = fixture.Services.CreateAsyncScope();
        var writeContext = scope.ServiceProvider.GetRequiredService<PlayersWriteDbContext>();
        var readContext = scope.ServiceProvider.GetRequiredService<PlayersReadDbContext>();

        var appliedMigrations = await writeContext.Database.GetAppliedMigrationsAsync(cancellationToken);
        var canRead = await readContext.Database.CanConnectAsync(cancellationToken);

        appliedMigrations.ShouldNotBeEmpty();
        canRead.ShouldBeTrue();
    }
}
