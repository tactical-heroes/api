using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetList;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.Mappers;

internal sealed partial class UserListItemReadModelSorting
    : IReadModelSorting<UserListItemReadModel>
{
    public static SortingParameters DefaultSorting { get; } =
        SortingParameters.Ascending(nameof(UserListItemReadModel.Email));
}
