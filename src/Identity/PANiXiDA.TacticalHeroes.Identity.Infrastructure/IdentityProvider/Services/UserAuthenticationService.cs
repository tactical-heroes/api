using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetAuthenticated;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Specifications;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Services;

public sealed class UserAuthenticationService(
    IUsersRepository usersRepository,
    IPasswordHashingService passwordHashingService,
    IUsersReadRepository usersReadRepository)
    : IUserAuthenticationService
{
    public async Task<Result<AuthenticatedUserReadModel>> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        var user = await usersRepository.GetBySpecificationAsync(
            new UserByEmailSpecification(email),
            cancellationToken);

        if (user is null ||
            !passwordHashingService.VerifyPassword(user.PasswordHash, password))
        {
            return InvalidCredentials();
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
            ? InvalidCredentials()
            : Result.Success(authenticatedUser);
    }

    private static Result<AuthenticatedUserReadModel> InvalidCredentials()
    {
        return Result.Failure<AuthenticatedUserReadModel>(
            Error.Unauthorized("Invalid credentials."));
    }
}
