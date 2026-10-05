using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetList;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.GetList;

internal sealed partial class UserListItemReadModelSorting
    : IReadModelSorting<UserListItemReadModel>
{
    public static SortingParameters DefaultSorting { get; } =
        SortingParameters.Of(
            new SortField(Field: nameof(UserListItemReadModel.Email)),
            new SortField(Field: nameof(UserListItemReadModel.Id), Order: SortDirection.Desc));
}
