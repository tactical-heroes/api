using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Heroes.Read.DbModels;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Heroes.Read.Mappers;

namespace PANiXiDA.TacticalHeroes.Compendium.IntegrationTests.Infrastructure.Persistence.Features.Heroes.Read.Mappers;

public sealed class HeroListItemReadModelMapperTests
{
    [Fact(DisplayName = "Hero list projection should return an empty faction name when faction is not loaded")]
    public void ProjectTo_Should_ReturnEmptyFactionName_When_FactionIsNotLoaded()
    {
        var query = new[] { new HeroReadDbModel() }.AsQueryable();

        var readModel = HeroListItemReadModelMapper.ProjectTo(query).Single();

        readModel.FactionName.ShouldBeEmpty();
    }
}
