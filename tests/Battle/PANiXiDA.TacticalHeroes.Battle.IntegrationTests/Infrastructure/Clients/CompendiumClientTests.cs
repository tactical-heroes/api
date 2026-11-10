using PANiXiDA.TacticalHeroes.Battle.Infrastructure.Clients;
using PANiXiDA.TacticalHeroes.Compendium.Contracts;
using PANiXiDA.TacticalHeroes.Compendium.Contracts.Heroes;
using PANiXiDA.TacticalHeroes.Compendium.Contracts.Units;

namespace PANiXiDA.TacticalHeroes.Battle.IntegrationTests.Infrastructure.Clients;

public sealed class CompendiumClientTests
{
    [Fact(DisplayName = "Client should preserve combat data when unit exists")]
    public async Task GetUnitByIdAsync_Should_PreserveCombatData_When_UnitExists()
    {
        var unit = new UnitResponse(
            Id: Guid.CreateVersion7(),
            Name: "Archer",
            Description: "Ranged unit",
            Attack: 8,
            Defense: 4,
            Health: 12,
            MinimumDamage: 3,
            MaximumDamage: 5,
            Initiative: 10.5,
            Speed: 5,
            Shots: 12,
            RangedAttackRange: 8,
            Morale: 1,
            Luck: 2,
            FactionId: Guid.CreateVersion7());
        var api = new StubCompendiumApi(unit, null);
        var client = new CompendiumClient(api);
        var ct = TestContext.Current.CancellationToken;

        var result = await client.GetUnitByIdAsync(unit.Id, ct);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(unit.Id);
        result.FactionId.ShouldBe(unit.FactionId);
        result.Attack.ShouldBe(unit.Attack);
        result.Defense.ShouldBe(unit.Defense);
        result.Health.ShouldBe(unit.Health);
        result.MinimumDamage.ShouldBe(unit.MinimumDamage);
        result.MaximumDamage.ShouldBe(unit.MaximumDamage);
        result.Initiative.ShouldBe(unit.Initiative);
        result.Speed.ShouldBe(unit.Speed);
        result.Shots.ShouldBe(unit.Shots);
        result.RangedAttackRange.ShouldBe(unit.RangedAttackRange);
        result.Morale.ShouldBe(unit.Morale);
        result.Luck.ShouldBe(unit.Luck);
        api.Id.ShouldBe(unit.Id);
        (api.CancellationToken == ct).ShouldBeTrue();
    }

    [Fact(DisplayName = "Client should preserve combat data when hero exists")]
    public async Task GetHeroByIdAsync_Should_PreserveCombatData_When_HeroExists()
    {
        var hero = new HeroResponse(
            Id: Guid.CreateVersion7(),
            Name: "Knight",
            Description: "Melee hero",
            Attack: 10,
            Defense: 6,
            MinimumDamage: 4,
            MaximumDamage: 8,
            Initiative: 12.5,
            Morale: 2,
            Luck: 1,
            FactionId: Guid.CreateVersion7());
        var api = new StubCompendiumApi(null, hero);
        var client = new CompendiumClient(api);
        var ct = TestContext.Current.CancellationToken;

        var result = await client.GetHeroByIdAsync(hero.Id, ct);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(hero.Id);
        result.FactionId.ShouldBe(hero.FactionId);
        result.Attack.ShouldBe(hero.Attack);
        result.Defense.ShouldBe(hero.Defense);
        result.MinimumDamage.ShouldBe(hero.MinimumDamage);
        result.MaximumDamage.ShouldBe(hero.MaximumDamage);
        result.Initiative.ShouldBe(hero.Initiative);
        result.Morale.ShouldBe(hero.Morale);
        result.Luck.ShouldBe(hero.Luck);
        api.Id.ShouldBe(hero.Id);
        (api.CancellationToken == ct).ShouldBeTrue();
    }

    [Fact(DisplayName = "Client should return null when unit does not exist")]
    public async Task GetUnitByIdAsync_Should_ReturnNull_When_UnitDoesNotExist()
    {
        var client = new CompendiumClient(new StubCompendiumApi(null, null));

        var result = await client.GetUnitByIdAsync(Guid.CreateVersion7(), TestContext.Current.CancellationToken);

        result.ShouldBeNull();
    }

    [Fact(DisplayName = "Client should return null when hero does not exist")]
    public async Task GetHeroByIdAsync_Should_ReturnNull_When_HeroDoesNotExist()
    {
        var client = new CompendiumClient(new StubCompendiumApi(null, null));

        var result = await client.GetHeroByIdAsync(Guid.CreateVersion7(), TestContext.Current.CancellationToken);

        result.ShouldBeNull();
    }

    private sealed class StubCompendiumApi(UnitResponse? unit, HeroResponse? hero) : ICompendiumApi
    {
        public Guid Id { get; private set; }
        public CancellationToken CancellationToken { get; private set; }

        public Task<UnitResponse?> GetUnitByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            Id = id;
            CancellationToken = cancellationToken;

            return Task.FromResult(unit);
        }

        public Task<HeroResponse?> GetHeroByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            Id = id;
            CancellationToken = cancellationToken;

            return Task.FromResult(hero);
        }
    }
}
