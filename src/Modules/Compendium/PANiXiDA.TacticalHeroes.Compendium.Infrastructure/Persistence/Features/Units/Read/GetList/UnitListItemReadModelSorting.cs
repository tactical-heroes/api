using PANiXiDA.TacticalHeroes.Compendium.Application.Units.GetList;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Units.Read.GetList;

internal sealed partial class UnitListItemReadModelSorting
    : IReadModelSorting<UnitListItemReadModel>
{
    public static SortingParameters DefaultSorting { get; } =
        SortingParameters.Of(
            new SortField(nameof(UnitListItemReadModel.Name)),
            new SortField(nameof(UnitListItemReadModel.Id), SortDirection.Desc));
}
