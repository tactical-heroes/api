using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetList;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Factions.Read.Mappers;

internal sealed partial class FactionListItemReadModelSorting
    : IReadModelSorting<FactionListItemReadModel>
{
    public static SortingParameters DefaultSorting { get; } =
        SortingParameters.Of(
            new SortField(nameof(FactionListItemReadModel.Name)),
            new SortField(nameof(FactionListItemReadModel.Id), SortDirection.Desc));
}
