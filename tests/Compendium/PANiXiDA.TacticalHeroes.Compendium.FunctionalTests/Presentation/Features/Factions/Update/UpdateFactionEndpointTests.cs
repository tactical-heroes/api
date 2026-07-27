using PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.Update;

namespace PANiXiDA.TacticalHeroes.Compendium.FunctionalTests.Presentation.Features.Factions.Update;

public sealed class UpdateFactionEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "PUT faction should update faction details")]
    public async Task PutFaction_Should_UpdateDetails_When_RequestIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new FactionsApiTestClient(Fixture);
        var createdFaction = await client.CreateAsync(cancellationToken);

        await client.UpdateAsync(
            createdFaction.Id,
            new UpdateFactionRequest(
                "Southern Alliance",
                "Defenders of the south."),
            cancellationToken);
        var faction = await client.GetDetailsAsync(
            createdFaction.Id,
            cancellationToken);

        faction.Name.ShouldBe("Southern Alliance");
        faction.Description.ShouldBe("Defenders of the south.");
    }
}
