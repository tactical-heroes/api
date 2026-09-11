using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Filters;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.ValueObjects;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Compendium.IntegrationTests.Infrastructure.Persistence.Features.Factions.Read;

public sealed class FactionsReadRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Theory(DisplayName = "GetSelectOptionsAsync should filter and limit sorted options when search is provided")]
    [InlineData(" aLLianCe ")]
    [InlineData("ALLIANCE")]
    public async Task GetSelectOptionsAsync_Should_FilterAndLimitSortedOptions_When_SearchIsProvided(string search)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var northern = CreateFaction("Northern Alliance", "North.");
        await AddFactionsAsync(cancellationToken,
            CreateFaction("Southern Alliance", "South."), northern,
            CreateFaction("Empire", "Alliance appears only in description."));
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFactionsReadRepository>();

        var options = await repository.GetSelectOptionsAsync(new FactionsFilter(search), 1, cancellationToken);

        options.ShouldHaveSingleItem();
        options[0].Id.ShouldBe(northern.Id.Value);
        options[0].Name.ShouldBe("Northern Alliance");
    }

    [Theory(DisplayName = "GetSelectOptionsAsync should return sorted options when search is blank")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetSelectOptionsAsync_Should_ReturnSortedOptions_When_SearchIsBlank(string? search)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await AddFactionsAsync(cancellationToken,
            CreateFaction("Southern Alliance", "South."),
            CreateFaction("Northern Alliance", "North."));
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFactionsReadRepository>();

        var options = await repository.GetSelectOptionsAsync(new FactionsFilter(search), 20, cancellationToken);

        options.Select(option => option.Name).ShouldBe(["Northern Alliance", "Southern Alliance"]);
    }

    [Fact(DisplayName = "GetSelectOptionsAsync should return empty options when no name matches")]
    public async Task GetSelectOptionsAsync_Should_ReturnEmptyOptions_When_NoNameMatches()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await AddFactionsAsync(cancellationToken, CreateFaction("Empire", "Northern Alliance"));
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFactionsReadRepository>();

        var options = await repository.GetSelectOptionsAsync(new FactionsFilter("Northern"), 20, cancellationToken);

        options.ShouldBeEmpty();
    }

    [Fact(DisplayName = "GetDetailsByIdAsync should return faction details when faction exists")]
    public async Task GetDetailsByIdAsync_Should_ReturnDetails_When_FactionExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var faction = CreateFaction(
            "Northern Alliance",
            "Defenders of the north.");
        await AddFactionsAsync(cancellationToken, faction);

        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFactionsReadRepository>();
        var details = await repository.GetDetailsByIdAsync(
            faction.Id.Value,
            cancellationToken);

        details.ShouldNotBeNull();
        details.Name.ShouldBe("Northern Alliance");
        details.Description.ShouldBe("Defenders of the north.");
    }

    [Fact(DisplayName = "GetPageAsync should return factions sorted by name when factions exist")]
    public async Task GetPageAsync_Should_ReturnSortedPage_When_FactionsExist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var northernFaction = CreateFaction(
            "Northern Alliance",
            "Defenders of the north.");
        var southernFaction = CreateFaction(
            "Southern Alliance",
            "Defenders of the south.");
        await AddFactionsAsync(
            cancellationToken,
            southernFaction,
            northernFaction);

        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFactionsReadRepository>();
        var page = await repository.GetPageAsync(
            new PaginationParameters(1, 20),
            cancellationToken);

        page.TotalCount.ShouldBe(2);
        page.Items.Select(item => item.Name)
            .ShouldBe(["Northern Alliance", "Southern Alliance"]);
    }

    [Fact(DisplayName = "ExistsByIdAsync should return true for an existing faction when faction exists")]
    public async Task ExistsByIdAsync_Should_ReturnTrue_When_FactionExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var faction = CreateFaction(
            "Northern Alliance",
            "Defenders of the north.");
        await AddFactionsAsync(cancellationToken, faction);

        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFactionsReadRepository>();

        (await repository.ExistsByIdAsync(faction.Id.Value, cancellationToken))
            .ShouldBeTrue();
    }

    [Fact(DisplayName = "AnyAsync should reflect whether factions exist when called")]
    public async Task AnyAsync_Should_ReflectWhetherFactionsExist_When_Called()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        await using (var emptyScope = Fixture.CreateScope())
        {
            var emptyRepository =
                emptyScope.ServiceProvider.GetRequiredService<IFactionsReadRepository>();
            (await emptyRepository.AnyAsync(cancellationToken)).ShouldBeFalse();
        }

        await AddFactionsAsync(
            cancellationToken,
            CreateFaction("Northern Alliance", "Defenders of the north."));

        await using var populatedScope = Fixture.CreateScope();
        var populatedRepository =
            populatedScope.ServiceProvider.GetRequiredService<IFactionsReadRepository>();
        (await populatedRepository.AnyAsync(cancellationToken)).ShouldBeTrue();
    }

    private static Faction CreateFaction(
        string name,
        string description)
    {
        return Faction.Create(
            name: FactionName.Create(value: name).Value,
            description: FactionDescription.Create(value: description).Value);
    }

    private async Task AddFactionsAsync(
        CancellationToken cancellationToken,
        params Faction[] factions)
    {
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFactionsRepository>();
        var dbContext = scope.ServiceProvider.GetRequiredService<CompendiumWriteDbContext>();

        foreach (var faction in factions)
        {
            await repository.AddAsync(faction, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
