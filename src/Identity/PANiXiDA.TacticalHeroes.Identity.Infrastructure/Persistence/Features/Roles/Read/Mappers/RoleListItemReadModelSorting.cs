using PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetList;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read.Mappers;

internal sealed partial class RoleListItemReadModelSorting
    : IReadModelSorting<RoleListItemReadModel>
{
    public static SortingParameters DefaultSorting { get; } =
        SortingParameters.Of(
            new SortField(nameof(RoleListItemReadModel.Name)),
            new SortField(nameof(RoleListItemReadModel.Id), SortDirection.Desc));
}
