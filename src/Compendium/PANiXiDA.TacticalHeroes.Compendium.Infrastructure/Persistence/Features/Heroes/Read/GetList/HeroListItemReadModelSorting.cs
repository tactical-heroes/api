using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetList;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Heroes.Read.GetList;

internal sealed partial class HeroListItemReadModelSorting
    : IReadModelSorting<HeroListItemReadModel>
{
    public static SortingParameters DefaultSorting { get; } =
        SortingParameters.Of(
            new SortField(Field: nameof(HeroListItemReadModel.Name)),
            new SortField(Field: nameof(HeroListItemReadModel.Id), Order: SortDirection.Desc));
}
