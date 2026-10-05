using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetList;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Factions.Read.GetList;

internal sealed partial class FactionListItemReadModelSorting
    : IReadModelSorting<FactionListItemReadModel>
{
    public static SortingParameters DefaultSorting { get; } =
        SortingParameters.Of(
            new SortField(Field: nameof(FactionListItemReadModel.Name)),
            new SortField(Field: nameof(FactionListItemReadModel.Id), Order: SortDirection.Desc));
}
