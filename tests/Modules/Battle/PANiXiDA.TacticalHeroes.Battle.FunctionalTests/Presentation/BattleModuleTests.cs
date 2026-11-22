using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Infrastructure.Messaging.Wolverine.OutboxDispatcher;

using PANiXiDA.TacticalHeroes.Battle.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Battle.FunctionalTests.Presentation;

[Collection(FunctionalTestCollectionDefinition.Name)]
public sealed class BattleModuleTests(FunctionalTestFixture fixture)
{
    [Fact(DisplayName = "Host should provide migrated Battle contexts and outbox when started")]
    public async Task Host_Should_ProvideMigratedBattleContextsAndOutbox_When_Started()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = fixture.Services.CreateAsyncScope();
        var writeContext = scope.ServiceProvider.GetRequiredService<BattleWriteDbContext>();
        var readContext = scope.ServiceProvider.GetRequiredService<BattleReadDbContext>();
        var outbox = scope.ServiceProvider.GetRequiredKeyedService<IOutboxDispatcher>(typeof(BattleWriteDbContext));

        var appliedMigrations = await writeContext.Database.GetAppliedMigrationsAsync(cancellationToken);
        var canRead = await readContext.Database.CanConnectAsync(cancellationToken);
        await outbox.FlushAsync(cancellationToken);

        appliedMigrations.ShouldNotBeEmpty();
        canRead.ShouldBeTrue();
    }
}
