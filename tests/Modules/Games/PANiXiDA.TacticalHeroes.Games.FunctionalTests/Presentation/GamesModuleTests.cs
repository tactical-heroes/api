using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Games.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Games.FunctionalTests.Presentation;

[Collection(FunctionalTestCollectionDefinition.Name)]
public sealed class GamesModuleTests(FunctionalTestFixture fixture)
{
    [Fact(DisplayName = "Host should provide migrated Games contexts when started")]
    public async Task Host_Should_ProvideMigratedGamesContexts_When_Started()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = fixture.Services.CreateAsyncScope();
        var writeContext = scope.ServiceProvider.GetRequiredService<GamesWriteDbContext>();
        var readContext = scope.ServiceProvider.GetRequiredService<GamesReadDbContext>();

        var appliedMigrations = await writeContext.Database.GetAppliedMigrationsAsync(cancellationToken);
        var canRead = await readContext.Database.CanConnectAsync(cancellationToken);

        appliedMigrations.ShouldNotBeEmpty();
        canRead.ShouldBeTrue();
    }
}
