using PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Units.Update;

namespace PANiXiDA.TacticalHeroes.Compendium.FunctionalTests.Presentation.Features.Units.Update;

public sealed class UpdateUnitEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "PUT unit should update details and faction when request is valid")]
    public async Task PutUnit_Should_UpdateDetailsAndFaction_When_RequestIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new UnitsApiTestClient(Fixture);
        var originalFaction = await client.CreateFactionAsync(cancellationToken);
        var targetFaction = await client.CreateFactionAsync(cancellationToken);
        var createdUnit = await client.CreateAsync(
            originalFaction.Id,
            cancellationToken);

        await client.UpdateAsync(
            createdUnit.Id,
            new UpdateUnitRequest(
                Name: "Swordsman",
                Description: "A disciplined melee unit.",
                Attack: 9,
                Defense: 10,
                Health: 20,
                MinimumDamage: 4,
                MaximumDamage: 6,
                Initiative: 9.5,
                Speed: 5,
                Shots: null,
                RangedAttackRange: null,
                Morale: 3,
                Luck: 2,
                FactionId: targetFaction.Id),
            cancellationToken);
        var unit = await client.GetDetailsAsync(
            createdUnit.Id,
            cancellationToken);

        unit.Name.ShouldBe("Swordsman");
        unit.Attack.ShouldBe(9);
        unit.Shots.ShouldBeNull();
        unit.RangedAttackRange.ShouldBeNull();
        unit.Morale.ShouldBe(3);
        unit.Luck.ShouldBe(2);
        unit.FactionId.ShouldBe(targetFaction.Id);
    }
}
