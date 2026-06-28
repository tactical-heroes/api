using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetAuthenticated;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Specifications;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Services;

public sealed class UserAuthenticationService(
    IUsersRepository usersRepository,
    IPasswordHashingService passwordHashingService,
    IUserClaimsProvider userClaimsProvider)
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

        return await CreateAuthenticatedUserAsync(user, cancellationToken);
    }

    private async Task<Result<AuthenticatedUserReadModel>> CreateAuthenticatedUserAsync(
        User user,
        CancellationToken cancellationToken)
    {
        var claims = await userClaimsProvider.GetClaimsAsync(user, cancellationToken);

        return Result.Success(
            AuthenticatedUserReadModel.Create(
                user.Id.Value,
                user.Email.Value,
                user.ConfirmationStatus.IsConfirmed,
                claims.Roles,
                claims.Claims));
    }

    private static Result<AuthenticatedUserReadModel> InvalidCredentials()
    {
        return Result.Failure<AuthenticatedUserReadModel>(
            Error.Unauthorized("Invalid credentials."));
    }
}
