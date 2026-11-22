using PANiXiDA.TacticalHeroes.Modules.Identity.Application.Roles.GetList;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Infrastructure.Persistence.Features.Roles.Read.GetList;

internal sealed partial class RoleListItemReadModelSorting
    : IReadModelSorting<RoleListItemReadModel>
{
    public static SortingParameters DefaultSorting { get; } =
        SortingParameters.Of(
            new SortField(nameof(RoleListItemReadModel.Name)),
            new SortField(nameof(RoleListItemReadModel.Id), SortDirection.Desc));
}
