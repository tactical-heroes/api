using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Specifications;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Authentication.Auth;

public sealed class AuthenticateUserHandler(
    IUsersRepository usersRepository,
    IPasswordHashingService passwordHashingService,
    IUsersReadRepository usersReadRepository)
    : ICommandHandler<AuthenticateUserCommand, Result<AuthenticatedUserReadModel>>
{
    public async Task<Result<AuthenticatedUserReadModel>> HandleAsync(
        AuthenticateUserCommand command,
        CancellationToken cancellationToken)
    {
        var user = await usersRepository.GetBySpecificationAsync(
            new UserByEmailSpecification(command.Email),
            cancellationToken);

        if (user is null ||
            !passwordHashingService.VerifyPassword(user.PasswordHash, command.Password))
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
