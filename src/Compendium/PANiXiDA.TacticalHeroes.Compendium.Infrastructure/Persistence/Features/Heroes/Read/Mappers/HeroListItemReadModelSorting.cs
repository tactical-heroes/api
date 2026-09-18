using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetList;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Heroes.Read.Mappers;

internal sealed partial class HeroListItemReadModelSorting
    : IReadModelSorting<HeroListItemReadModel>
{
    public static SortingParameters DefaultSorting { get; } =
        SortingParameters.Ascending(nameof(HeroListItemReadModel.Name));
}
