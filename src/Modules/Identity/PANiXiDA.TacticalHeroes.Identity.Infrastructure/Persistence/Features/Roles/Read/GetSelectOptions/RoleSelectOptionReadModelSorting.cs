using PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetSelectOptions;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read.GetSelectOptions;

internal sealed partial class RoleSelectOptionReadModelSorting : IReadModelSorting<RoleSelectOptionReadModel>
{
    public static SortingParameters DefaultSorting { get; } =
        SortingParameters.Of(
            new SortField(nameof(RoleSelectOptionReadModel.Name)),
            new SortField(nameof(RoleSelectOptionReadModel.Id), SortDirection.Desc));
}
