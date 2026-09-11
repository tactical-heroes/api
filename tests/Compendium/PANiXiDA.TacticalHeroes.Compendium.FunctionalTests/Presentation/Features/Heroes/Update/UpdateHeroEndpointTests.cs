using PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Heroes.Update;

namespace PANiXiDA.TacticalHeroes.Compendium.FunctionalTests.Presentation.Features.Heroes.Update;

public sealed class UpdateHeroEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "PUT hero should update details and faction when request is valid")]
    public async Task PutHero_Should_UpdateDetailsAndFaction_When_RequestIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new HeroesApiTestClient(Fixture);
        var originalFaction = await client.CreateFactionAsync(cancellationToken);
        var targetFaction = await client.CreateFactionAsync(cancellationToken);
        var createdHero = await client.CreateAsync(
            originalFaction.Id,
            cancellationToken);

        await client.UpdateAsync(
            createdHero.Id,
            new UpdateHeroRequest(
                Name: "Elara",
                Description: "An agile vanguard commander.",
                Attack: 10,
                Defense: 7,
                MinimumDamage: 4,
                MaximumDamage: 9,
                Initiative: 12.25,
                Morale: 5,
                Luck: 3,
                FactionId: targetFaction.Id),
            cancellationToken);
        var hero = await client.GetDetailsAsync(
            createdHero.Id,
            cancellationToken);

        hero.Name.ShouldBe("Elara");
        hero.Attack.ShouldBe(10);
        hero.MaximumDamage.ShouldBe(9);
        hero.Morale.ShouldBe(5);
        hero.Luck.ShouldBe(3);
        hero.FactionId.ShouldBe(targetFaction.Id);
    }
}
