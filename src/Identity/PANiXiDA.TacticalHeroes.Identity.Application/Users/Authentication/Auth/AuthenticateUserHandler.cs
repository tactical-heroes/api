using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Authentication.Auth;

public sealed class AuthenticateUserHandler(
    IUsersRepository usersRepository,
    IUserCredentialsService userCredentialsService,
    IUsersReadRepository usersReadRepository)
    : ICommandHandler<AuthenticateUserCommand, Result<AuthenticatedUserReadModel>>
{
    public async Task<Result<AuthenticatedUserReadModel>> HandleAsync(
        AuthenticateUserCommand command,
        CancellationToken cancellationToken)
    {
        var user = await usersRepository.GetByEmailAsync(command.Email, cancellationToken);

        if (user is null ||
            !await userCredentialsService.CheckPasswordAsync(user, command.Password, cancellationToken))
        {
            return Result.Failure<AuthenticatedUserReadModel>(
                Error.Unauthorized("Invalid credentials."));
        }

        if (!user.ConfirmationStatus.IsConfirmed)
        {
            return Result.Failure<AuthenticatedUserReadModel>(
                Error.Forbidden("Account is not confirmed."));
        }

        var authenticatedUser = await usersReadRepository.GetAuthenticatedUserByIdAsync(
            user.Id.Value,
            cancellationToken);

        return authenticatedUser is null
            ? Result.Failure<AuthenticatedUserReadModel>(
                Error.Unauthorized("Invalid credentials."))
            : Result.Success(authenticatedUser);
    }
}
