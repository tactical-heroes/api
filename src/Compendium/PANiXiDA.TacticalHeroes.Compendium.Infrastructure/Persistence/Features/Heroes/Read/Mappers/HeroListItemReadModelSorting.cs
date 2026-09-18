using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetList;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Heroes.Read.Mappers;

internal sealed partial class HeroListItemReadModelSorting
    : IReadModelSorting<HeroListItemReadModel>
{
    public static SortingParameters DefaultSorting { get; } =
        SortingParameters.Of(
            new SortField(nameof(HeroListItemReadModel.Name)),
            new SortField(nameof(HeroListItemReadModel.Id), SortDirection.Desc));
}
