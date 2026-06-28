using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Authentication.Refresh;

public sealed class RefreshAuthenticationHandler(
    IUsersReadRepository usersReadRepository)
    : ICommandHandler<RefreshAuthenticationCommand, Result<AuthenticatedUserReadModel>>
{
    public async Task<Result<AuthenticatedUserReadModel>> HandleAsync(
        RefreshAuthenticationCommand command,
        CancellationToken cancellationToken)
    {
        var user = await usersReadRepository.GetAuthenticatedUserByIdAsync(
            command.UserId,
            cancellationToken);

        if (user is null)
        {
            return Result.Failure<AuthenticatedUserReadModel>(
                Error.NotFound("User was not found."));
        }

        if (!user.ConfirmationStatus)
        {
            return Result.Failure<AuthenticatedUserReadModel>(
                Error.Forbidden("Account is not confirmed."));
        }

        return Result.Success(user);
    }
}
