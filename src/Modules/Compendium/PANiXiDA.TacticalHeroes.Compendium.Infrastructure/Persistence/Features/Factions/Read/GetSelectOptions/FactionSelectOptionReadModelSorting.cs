using PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Factions.GetSelectOptions;

namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Infrastructure.Persistence.Features.Factions.Read.GetSelectOptions;

internal sealed partial class FactionSelectOptionReadModelSorting
    : IReadModelSorting<FactionSelectOptionReadModel>
{
    public static SortingParameters DefaultSorting { get; } =
        SortingParameters.Of(
            new SortField(nameof(FactionSelectOptionReadModel.Name)),
            new SortField(nameof(FactionSelectOptionReadModel.Id), SortDirection.Desc));
}
