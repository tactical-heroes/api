using System.Text.Json;

using PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Units.Update;

namespace PANiXiDA.TacticalHeroes.Compendium.FunctionalTests.Presentation.Features.Units.Update;

public sealed class UpdateUnitEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Theory(DisplayName = "PUT unit should update details and faction when ranged attack fields are omitted or null")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PutUnit_Should_UpdateDetailsAndFaction_When_RangedAttackFieldsAreOmittedOrNull(bool omitFields)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new UnitsApiTestClient(Fixture);
        var originalFaction = await client.CreateFactionAsync(cancellationToken);
        var targetFaction = await client.CreateFactionAsync(cancellationToken);
        var createdUnit = await client.CreateAsync(
            originalFaction.Id,
            cancellationToken);

        var request = new UpdateUnitRequest(
            Name: "Swordsman",
            Description: "A disciplined melee unit.",
            Attack: 9,
            Defense: 10,
            Health: 20,
            MinimumDamage: 4,
            MaximumDamage: 6,
            Initiative: 9.5,
            Speed: 5,
            Morale: 3,
            Luck: 2,
            FactionId: targetFaction.Id,
            Shots: null,
            RangedAttackRange: null);
        var payload = JsonSerializer.SerializeToNode(request, TestJsonSerializerOptions.Web)!.AsObject();
        if (omitFields)
        {
            payload.Remove("shots");
            payload.Remove("rangedAttackRange");
        }

        using var response = await Fixture.Client.PutAsJsonAsync(
            $"/api/v1/units/{createdUnit.Id}", payload, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent, responseBody);
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
