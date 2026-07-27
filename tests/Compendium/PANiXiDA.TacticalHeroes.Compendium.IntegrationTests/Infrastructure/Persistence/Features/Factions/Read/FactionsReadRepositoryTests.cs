using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Compendium.IntegrationTests.Infrastructure.Persistence.Features.Factions.Read;

public sealed class FactionsReadRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "Factions read repository should return details and a sorted page")]
    public async Task Repository_Should_ReturnDetailsAndSortedPage_When_FactionsExist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var northernFaction = Faction.Create(
            "Northern Alliance",
            "Defenders of the north.").Value;
        var southernFaction = Faction.Create(
            "Southern Alliance",
            "Defenders of the south.").Value;

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IFactionsRepository>();
            var dbContext = scope.ServiceProvider.GetRequiredService<CompendiumWriteDbContext>();

            await repository.AddAsync(southernFaction, cancellationToken);
            await repository.AddAsync(northernFaction, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        await using var verificationScope = Fixture.CreateScope();
        var factionsReadRepository =
            verificationScope.ServiceProvider.GetRequiredService<IFactionsReadRepository>();
        var details = await factionsReadRepository.GetDetailsByIdAsync(
            northernFaction.Id.Value,
            cancellationToken);
        var page = await factionsReadRepository.GetPagedAsync(
            new PaginationParameters(1, 20),
            cancellationToken);

        details.ShouldNotBeNull();
        details.Name.ShouldBe("Northern Alliance");
        details.Description.ShouldBe("Defenders of the north.");
        page.TotalCount.ShouldBe(2);
        page.Items.Select(item => item.Name)
            .ShouldBe(["Northern Alliance", "Southern Alliance"]);
        (await factionsReadRepository.ExistsByIdAsync(
                northernFaction.Id.Value,
                cancellationToken))
            .ShouldBeTrue();
    }
}
