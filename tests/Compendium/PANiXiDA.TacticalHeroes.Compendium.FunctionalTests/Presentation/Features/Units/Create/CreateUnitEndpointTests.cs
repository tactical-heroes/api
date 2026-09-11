using PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Units.Create;

namespace PANiXiDA.TacticalHeroes.Compendium.FunctionalTests.Presentation.Features.Units.Create;

public sealed class CreateUnitEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "POST units should create a normalized ranged unit when request is valid")]
    public async Task PostUnits_Should_CreateUnit_When_RequestIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new UnitsApiTestClient(Fixture);
        var faction = await client.CreateFactionAsync(cancellationToken);

        var createdUnit = await client.CreateAsync(
            faction.Id,
            cancellationToken,
            new CreateUnitRequest(
                Name: "  Archer  ",
                Description: "  A disciplined ranged unit.  ",
                Attack: 8,
                Defense: 4,
                Health: 12,
                MinimumDamage: 3,
                MaximumDamage: 5,
                Initiative: 10.5,
                Speed: 6,
                Shots: 12,
                RangedAttackRange: 8,
                Morale: 2,
                Luck: 1,
                FactionId: faction.Id));
        var unit = await client.GetDetailsAsync(
            createdUnit.Id,
            cancellationToken);

        unit.Name.ShouldBe("Archer");
        unit.Description.ShouldBe("A disciplined ranged unit.");
        unit.Attack.ShouldBe(8);
        unit.Defense.ShouldBe(4);
        unit.Health.ShouldBe(12);
        unit.MinimumDamage.ShouldBe(3);
        unit.MaximumDamage.ShouldBe(5);
        unit.Initiative.ShouldBe(10.5);
        unit.Speed.ShouldBe(6);
        unit.Shots.ShouldBe(12);
        unit.RangedAttackRange.ShouldBe(8);
        unit.Morale.ShouldBe(2);
        unit.Luck.ShouldBe(1);
        unit.FactionId.ShouldBe(faction.Id);
    }
}
