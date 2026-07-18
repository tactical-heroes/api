using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.GetDetails;

public sealed class GetUserDetailsHandler(IUsersReadRepository usersRepository)
    : IQueryHandler<GetUserDetailsQuery, Result<UserDetailsReadModel>>
{
    public async Task<Result<UserDetailsReadModel>> HandleAsync(
        GetUserDetailsQuery query,
        CancellationToken cancellationToken)
    {
        var user = await usersRepository.GetDetailsByIdAsync(
            id: query.Id,
            cancellationToken: cancellationToken);

        return user is null
            ? Result.Failure<UserDetailsReadModel>(
                error: Error.NotFound(message: "User was not found."))
            : Result.Success(value: user);
    }
}
