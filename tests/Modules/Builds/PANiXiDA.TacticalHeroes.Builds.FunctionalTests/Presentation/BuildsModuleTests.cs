using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Builds.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Builds.FunctionalTests.Presentation;

[Collection(FunctionalTestCollectionDefinition.Name)]
public sealed class BuildsModuleTests(FunctionalTestFixture fixture)
{
    [Fact(DisplayName = "Host should provide migrated Builds contexts when started")]
    public async Task Host_Should_ProvideMigratedBuildsContexts_When_Started()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = fixture.Services.CreateAsyncScope();
        var writeContext = scope.ServiceProvider.GetRequiredService<BuildsWriteDbContext>();
        var readContext = scope.ServiceProvider.GetRequiredService<BuildsReadDbContext>();

        var appliedMigrations = await writeContext.Database.GetAppliedMigrationsAsync(cancellationToken);
        var canRead = await readContext.Database.CanConnectAsync(cancellationToken);

        appliedMigrations.ShouldNotBeEmpty();
        canRead.ShouldBeTrue();
    }
}
