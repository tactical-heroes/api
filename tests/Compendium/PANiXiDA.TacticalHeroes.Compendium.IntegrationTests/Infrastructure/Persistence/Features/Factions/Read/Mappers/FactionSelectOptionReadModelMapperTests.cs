using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Factions.Read.DbModels;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Factions.Read.Mappers;

namespace PANiXiDA.TacticalHeroes.Compendium.IntegrationTests.Infrastructure.Persistence.Features.Factions.Read.Mappers;

public sealed class FactionSelectOptionReadModelMapperTests
{
    [Fact(DisplayName = "ProjectTo should project option when faction exists")]
    public void ProjectTo_Should_ProjectOption_When_FactionExists()
    {
        var faction = new FactionReadDbModel { Id = Guid.NewGuid(), Name = "Northern Alliance", Description = "A faction." };

        var option = FactionSelectOptionReadModelMapper.ProjectTo(new[] { faction }.AsQueryable()).Single();

        option.Id.ShouldBe(faction.Id);
        option.Name.ShouldBe(faction.Name);
    }
}
