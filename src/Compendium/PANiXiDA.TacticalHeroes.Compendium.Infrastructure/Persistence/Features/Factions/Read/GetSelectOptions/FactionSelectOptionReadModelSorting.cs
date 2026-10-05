using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetSelectOptions;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Factions.Read.GetSelectOptions;

internal sealed partial class FactionSelectOptionReadModelSorting
    : IReadModelSorting<FactionSelectOptionReadModel>
{
    public static SortingParameters DefaultSorting { get; } =
        SortingParameters.Of(
            new SortField(Field: nameof(FactionSelectOptionReadModel.Name)),
            new SortField(Field: nameof(FactionSelectOptionReadModel.Id), Order: SortDirection.Desc));
}
