using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Battle.Application.Battles.Abstractions;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Abstractions;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units.ValueObjects;
using PANiXiDA.TacticalHeroes.Battle.Infrastructure.Persistence.Core;

using BattleAggregate = PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Battle;

namespace PANiXiDA.TacticalHeroes.Battle.IntegrationTests.Infrastructure.Persistence.Features.Battles.Read;

[Collection(IntegrationTestCollectionDefinition.Name)]
public sealed class BattlesReadRepositoryTests(IntegrationTestFixture fixture)
{
    [Fact(DisplayName = "Read repository should find only stored battles when only one battle is stored")]
    public async Task ExistsByIdAsync_Should_FindOnlyStoredBattles_When_OnlyOneBattleIsStored()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(cancellationToken);
        var battle = BattleAggregate.Create([CreateUnit(), CreateUnit()]).Value;
        await using (var writeScope = fixture.CreateScope())
        {
            writeScope.ServiceProvider.GetRequiredService<IBattlesRepository>().Add(battle);
            await writeScope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(BattleWriteDbContext))
                .SaveChangesAsync(cancellationToken);
        }

        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBattlesReadRepository>();
        var storedExists = await repository.ExistsByIdAsync(battle.Id.Value, cancellationToken);
        var missingExists = await repository.ExistsByIdAsync(Guid.CreateVersion7(), cancellationToken);

        storedExists.ShouldBeTrue();
        missingExists.ShouldBeFalse();
        (await repository.AnyAsync(cancellationToken)).ShouldBeTrue();
    }

    [Fact(DisplayName = "Read repository should report no battles when battles are absent")]
    public async Task AnyAsync_Should_ReturnFalse_When_BattlesAreAbsent()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(cancellationToken);
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBattlesReadRepository>();

        var result = await repository.AnyAsync(cancellationToken);

        result.ShouldBeFalse();
    }

    private static Unit CreateUnit()
    {
        return Unit.Create(
            UnitCombatStats.Create(0, 0, 10).Value,
            DamageRange.Create(1, 1).Value,
            UnitStackState.Create(1, 10).Value).Value;
    }
}
