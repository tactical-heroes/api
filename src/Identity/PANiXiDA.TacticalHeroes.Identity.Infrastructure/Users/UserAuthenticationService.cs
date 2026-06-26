using PANiXiDA.TacticalHeroes.Identity.Application.Users;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Users;

public sealed class UserAuthenticationService(
    IUsersRepository usersRepository,
    IPasswordHashingService passwordHashingService,
    IUserClaimsProvider userClaimsProvider)
    : IUserAuthenticationService
{
    public async Task<Result<AuthenticatedUser>> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        var user = await usersRepository.GetByEmailAsync(
            email,
            cancellationToken);

        if (user is null ||
            !passwordHashingService.VerifyPassword(user.PasswordHash, password))
        {
            return InvalidCredentials();
        }

        if (!user.IsConfirmed)
        {
            return Result.Failure<AuthenticatedUser>(
                Error.Forbidden("Account is not confirmed."));
        }

        return await CreateAuthenticatedUserAsync(user, cancellationToken);
    }

    public async Task<Result<AuthenticatedUser>> GetConfirmedUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var userIdResult = UserId.Create(userId);

        if (userIdResult.IsFailure)
        {
            return Result.Failure<AuthenticatedUser>(userIdResult.Errors);
        }

        var user = await usersRepository.GetByIdAsync(
            userIdResult.Value,
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

        return await CreateAuthenticatedUserAsync(user, cancellationToken);
    }

    private async Task<Result<AuthenticatedUser>> CreateAuthenticatedUserAsync(
        User user,
        CancellationToken cancellationToken)
    {
        var claims = await userClaimsProvider.GetClaimsAsync(user, cancellationToken);

        return Result.Success(
            new AuthenticatedUser(
                user.Id.Value,
                user.Email.Value,
                user.IsConfirmed,
                claims.Roles,
                claims.Claims));
    }

    private static Result<AuthenticatedUser> InvalidCredentials()
    {
        return Result.Failure<AuthenticatedUser>(
            Error.Unauthorized("Invalid credentials."));
    }
}
