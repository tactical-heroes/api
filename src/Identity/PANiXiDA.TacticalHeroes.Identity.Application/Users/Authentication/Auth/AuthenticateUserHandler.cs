using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Authentication.Auth;

public sealed class AuthenticateUserHandler(
    IUserCredentialsService userCredentialsService,
    IUsersReadRepository usersReadRepository)
    : ICommandHandler<AuthenticateUserCommand, Result<AuthenticatedUserReadModel>>
{
    public async Task<Result<AuthenticatedUserReadModel>> HandleAsync(
        AuthenticateUserCommand command,
        CancellationToken cancellationToken)
    {
        var authenticationResult = await userCredentialsService.AuthenticateAsync(
            command.Email,
            command.Password,
            cancellationToken);

        if (authenticationResult.IsFailure)
        {
            return Result.Failure<AuthenticatedUserReadModel>(authenticationResult.Errors);
        }

        var authenticatedUser = await usersReadRepository.GetAuthenticatedUserByIdAsync(
            authenticationResult.Value,
            cancellationToken);

        return authenticatedUser is null
            ? Result.Failure<AuthenticatedUserReadModel>(
                Error.Unauthorized("Invalid credentials."))
            : Result.Success(authenticatedUser);
    }
}
