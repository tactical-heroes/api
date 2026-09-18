using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetList;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Factions.Read.Mappers;

internal sealed partial class FactionListItemReadModelSorting
    : IReadModelSorting<FactionListItemReadModel>
{
    public static SortingParameters DefaultSorting { get; } =
        SortingParameters.Ascending(nameof(FactionListItemReadModel.Name));
}
