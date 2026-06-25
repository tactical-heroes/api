using PANiXiDA.TacticalHeroes.Identity.Application.IdentityUsers;
using PANiXiDA.TacticalHeroes.Identity.Application.IdentityUsers.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers;
using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityUsers;

public sealed class IdentityAuthenticationService(
    IIdentityUsersRepository identityUsersRepository,
    IPasswordHashingService passwordHashingService,
    IIdentityClaimsProvider identityClaimsProvider)
    : IIdentityAuthenticationService
{
    public async Task<Result<AuthenticatedIdentityUser>> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(email);

        if (emailResult.IsFailure)
        {
            return InvalidCredentials();
        }

        var user = await identityUsersRepository.GetByEmailAsync(
            emailResult.Value,
            cancellationToken);

        if (user is null ||
            !passwordHashingService.VerifyPassword(user.PasswordHash, password))
        {
            return InvalidCredentials();
        }

        if (!user.IsConfirmed)
        {
            return Result.Failure<AuthenticatedIdentityUser>(
                Error.Forbidden("Account is not confirmed."));
        }

        return await CreateAuthenticatedUserAsync(user, cancellationToken);
    }

    public async Task<Result<AuthenticatedIdentityUser>> GetConfirmedUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var userIdResult = IdentityUserId.Create(userId);

        if (userIdResult.IsFailure)
        {
            return Result.Failure<AuthenticatedIdentityUser>(userIdResult.Errors);
        }

        var user = await identityUsersRepository.GetByIdAsync(
            userIdResult.Value,
            cancellationToken);

        if (user is null)
        {
            return Result.Failure<AuthenticatedIdentityUser>(
                Error.NotFound("Identity user was not found."));
        }

        if (!user.IsConfirmed)
        {
            return Result.Failure<AuthenticatedIdentityUser>(
                Error.Forbidden("Account is not confirmed."));
        }

        return await CreateAuthenticatedUserAsync(user, cancellationToken);
    }

    private async Task<Result<AuthenticatedIdentityUser>> CreateAuthenticatedUserAsync(
        IdentityUser user,
        CancellationToken cancellationToken)
    {
        var claims = await identityClaimsProvider.GetClaimsAsync(user, cancellationToken);

        return Result.Success(
            new AuthenticatedIdentityUser(
                user.Id.Value,
                user.Email.Value,
                user.IsConfirmed,
                claims.Roles,
                claims.Permissions));
    }

    private static Result<AuthenticatedIdentityUser> InvalidCredentials()
    {
        return Result.Failure<AuthenticatedIdentityUser>(
            Error.Unauthorized("Invalid credentials."));
    }
}
