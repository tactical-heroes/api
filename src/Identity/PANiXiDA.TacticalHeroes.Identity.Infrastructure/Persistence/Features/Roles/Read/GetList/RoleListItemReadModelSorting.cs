using PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetList;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read.GetList;

internal sealed partial class RoleListItemReadModelSorting
    : IReadModelSorting<RoleListItemReadModel>
{
    public static SortingParameters DefaultSorting { get; } =
        SortingParameters.Of(
            new SortField(Field: nameof(RoleListItemReadModel.Name)),
            new SortField(Field: nameof(RoleListItemReadModel.Id), Order: SortDirection.Desc));
}
