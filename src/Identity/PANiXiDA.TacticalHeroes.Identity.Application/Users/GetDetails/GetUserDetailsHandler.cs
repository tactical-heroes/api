using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.GetDetails;

public sealed class GetUserDetailsHandler(IUsersReadRepository usersReadRepository)
    : IQueryHandler<GetUserDetailsQuery, Result<UserDetailsReadModel>>, IRequireAuthorization
{
    public static IReadOnlyCollection<string> AllPermissions { get; } =
    [
        Permissions.IdentityUsers.Read.Code
    ];

    public async Task<Result<UserDetailsReadModel>> HandleAsync(
        GetUserDetailsQuery query,
        CancellationToken cancellationToken)
    {
        var user = await usersReadRepository.GetDetailsByIdAsync(
            id: query.Id,
            cancellationToken: cancellationToken);

        return user is null
            ? Result.Failure<UserDetailsReadModel>(
                error: Error.NotFound(message: "User was not found."))
            : Result.Success(value: user);
    }
}
