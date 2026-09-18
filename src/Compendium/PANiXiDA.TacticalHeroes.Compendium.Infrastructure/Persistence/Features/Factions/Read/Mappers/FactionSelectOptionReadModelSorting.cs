using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetSelectOptions;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Factions.Read.Mappers;

internal sealed partial class FactionSelectOptionReadModelSorting
    : IReadModelSorting<FactionSelectOptionReadModel>
{
    public static SortingParameters DefaultSorting { get; } =
        SortingParameters.Of(
            new SortField(nameof(FactionSelectOptionReadModel.Name)),
            new SortField(nameof(FactionSelectOptionReadModel.Id), SortDirection.Desc));
}
