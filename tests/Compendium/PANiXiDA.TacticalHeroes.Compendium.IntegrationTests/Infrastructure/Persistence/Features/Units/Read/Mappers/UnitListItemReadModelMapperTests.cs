using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Units.Read.DbModels;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Units.Read.Mappers;

namespace PANiXiDA.TacticalHeroes.Compendium.IntegrationTests.Infrastructure.Persistence.Features.Units.Read.Mappers;

public sealed class UnitListItemReadModelMapperTests
{
    [Fact(DisplayName = "Unit list projection should return an empty faction name when faction is not loaded")]
    public void ProjectTo_Should_ReturnEmptyFactionName_When_FactionIsNotLoaded()
    {
        var query = new[] { new UnitReadDbModel() }.AsQueryable();

        var readModel = UnitListItemReadModelMapper.ProjectTo(query).Single();

        readModel.FactionName.ShouldBeEmpty();
    }
}
