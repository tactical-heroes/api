using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.GetAuthenticated;

public sealed class GetAuthenticatedUserHandler(
    IUsersReadRepository usersReadRepository)
    : IQueryHandler<GetAuthenticatedUserQuery, Result<AuthenticatedUser>>
{
    public async Task<Result<AuthenticatedUser>> HandleAsync(
        GetAuthenticatedUserQuery query,
        CancellationToken cancellationToken)
    {
        var user = await usersReadRepository.GetAuthenticatedUserByIdAsync(
            query.UserId,
            cancellationToken);

        if (user is null)
        {
            return Result.Failure<AuthenticatedUser>(
                Error.NotFound("User was not found."));
        }

        if (!user.IsConfirmed)
        {
            return Result.Failure<AuthenticatedUser>(
                Error.Forbidden("Account is not confirmed."));
        }

        return Result.Success(user);
    }
}
