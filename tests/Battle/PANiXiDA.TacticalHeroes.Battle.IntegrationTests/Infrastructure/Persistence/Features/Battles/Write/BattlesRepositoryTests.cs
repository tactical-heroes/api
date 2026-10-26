using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Persistence.Errors;

using PANiXiDA.TacticalHeroes.Battle.Application.Battles.Abstractions;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Abstractions;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units.ValueObjects;
using PANiXiDA.TacticalHeroes.Battle.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Battle.Infrastructure.Persistence.Features.Battles.Read.DbModels;

using BattleAggregate = PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Battle;

namespace PANiXiDA.TacticalHeroes.Battle.IntegrationTests.Infrastructure.Persistence.Features.Battles.Write;

[Collection(IntegrationTestCollectionDefinition.Name)]
public sealed class BattlesRepositoryTests(IntegrationTestFixture fixture)
{
    [Fact(DisplayName = "Battle persistence should preserve damage and membership when a battle is reloaded")]
    public async Task GetByIdAsync_Should_PreserveDamageAndMembership_When_ABattleIsReloaded()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(cancellationToken);
        var battle = BattleAggregate.Create();
        var attacker = CreateUnit(battle.Id, 7, 1, 10);
        var defender = CreateUnit(battle.Id, 1, 3, 4);
        battle.AddUnits([attacker, defender]).IsSuccess.ShouldBeTrue();

        await using (var scope = fixture.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<IBattlesRepository>().Add(battle);
            await GetUnitOfWork(scope.ServiceProvider).SaveChangesAsync(cancellationToken);
        }

        await using (var scope = fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IBattlesRepository>();
            var restored = (await repository.GetByIdAsync(battle.Id, cancellationToken))!;
            restored.Units.Count.ShouldBe(2);
            restored.Units.ShouldAllBe(unit => unit.BattleId == battle.Id);
            restored.Attack(attacker.Id, defender.Id).IsSuccess.ShouldBeTrue();
            repository.Update(restored);
            await GetUnitOfWork(scope.ServiceProvider).SaveChangesAsync(cancellationToken);
        }

        await using var readScope = fixture.CreateScope();
        var readContext = readScope.ServiceProvider.GetRequiredService<BattleReadDbContext>();
        var readRepository = readScope.ServiceProvider.GetRequiredService<IBattlesReadRepository>();
        var persisted = await readContext.Set<BattleReadDbModel>()
            .Include(value => value.Units)
            .SingleAsync(value => value.Id == battle.Id.Value, cancellationToken);
        var target = persisted.Units.Single(unit => unit.Id == defender.Id.Value);

        target.StackStateCount.ShouldBe(2);
        target.StackStateCurrentUnitHealth.ShouldBe(7);
        target.StatsMaxHealth.ShouldBe(10);
        (await readRepository.ExistsByIdAsync(battle.Id.Value, cancellationToken)).ShouldBeTrue();
        (await readRepository.AnyAsync(cancellationToken)).ShouldBeTrue();
    }

    [Fact(DisplayName = "Battle persistence should persist added units when a battle already exists")]
    public async Task Update_Should_PersistAddedUnits_When_ABattleAlreadyExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(cancellationToken);
        var battle = BattleAggregate.Create();
        var unit = CreateUnit(battle.Id, 2, 1, 10);
        await using (var scope = fixture.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<IBattlesRepository>().Add(battle);
            await GetUnitOfWork(scope.ServiceProvider).SaveChangesAsync(cancellationToken);
        }

        await using (var scope = fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IBattlesRepository>();
            var restored = (await repository.GetByIdAsync(battle.Id, cancellationToken))!;
            restored.AddUnits([unit]).IsSuccess.ShouldBeTrue();
            repository.Update(restored);
            await GetUnitOfWork(scope.ServiceProvider).SaveChangesAsync(cancellationToken);
        }

        await using var readScope = fixture.CreateScope();
        var reloaded = (await readScope.ServiceProvider.GetRequiredService<IBattlesRepository>()
            .GetByIdAsync(battle.Id, cancellationToken))!;

        var restoredUnit = reloaded.Units.ShouldHaveSingleItem();
        restoredUnit.Id.ShouldBe(unit.Id);
        restoredUnit.BattleId.ShouldBe(battle.Id);
        restoredUnit.DamageRange.Minimum.ShouldBe(2);
        restoredUnit.Count.ShouldBe(1);
        restoredUnit.CurrentUnitHealth.ShouldBe(10);
    }

    [Fact(DisplayName = "Battle persistence should reject stale updates when concurrent damage is saved")]
    public async Task Update_Should_RejectStaleUpdates_When_ConcurrentDamageIsSaved()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(cancellationToken);
        var battle = BattleAggregate.Create();
        var attacker = CreateUnit(battle.Id, 1, 1, 10);
        var defender = CreateUnit(battle.Id, 1, 3, 10);
        battle.AddUnits([attacker, defender]).IsSuccess.ShouldBeTrue();
        await using (var scope = fixture.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<IBattlesRepository>().Add(battle);
            await GetUnitOfWork(scope.ServiceProvider).SaveChangesAsync(cancellationToken);
        }

        await using var firstScope = fixture.CreateScope();
        await using var secondScope = fixture.CreateScope();
        var firstRepository = firstScope.ServiceProvider.GetRequiredService<IBattlesRepository>();
        var secondRepository = secondScope.ServiceProvider.GetRequiredService<IBattlesRepository>();
        var first = (await firstRepository.GetByIdAsync(battle.Id, cancellationToken))!;
        var second = (await secondRepository.GetByIdAsync(battle.Id, cancellationToken))!;

        first.Attack(attacker.Id, defender.Id);
        firstRepository.Update(first);
        await GetUnitOfWork(firstScope.ServiceProvider).SaveChangesAsync(cancellationToken);
        second.Attack(attacker.Id, defender.Id);
        secondRepository.Update(second);
        Task SaveStale()
        {
            return GetUnitOfWork(secondScope.ServiceProvider).SaveChangesAsync(cancellationToken);
        }

        var exception = await Should.ThrowAsync<ConcurrencyException>(SaveStale);

        exception.InnerException.ShouldBeOfType<DbUpdateConcurrencyException>();
    }

    private static IUnitOfWork GetUnitOfWork(IServiceProvider services)
    {
        return services.GetRequiredKeyedService<IUnitOfWork>(typeof(BattleWriteDbContext));
    }

    private static Unit CreateUnit(BattleId battleId, int damage, int count, int currentHealth)
    {
        return Unit.Create(
            battleId,
            UnitCombatStats.Create(0, 0, 10).Value,
            DamageRange.Create(damage, damage).Value,
            UnitStackState.Create(count, currentHealth).Value).Value;
    }
}
