using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Mappers;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Services;

public sealed class UserCredentialsService(
    UserManager<ApplicationUser> userManager,
    IOptions<IdentityProviderOptions> options,
    TimeProvider timeProvider)
    : IUserCredentialsService
{
    public async Task<bool> CheckPasswordAsync(
        User user,
        string password,
        CancellationToken cancellationToken)
    {
        var applicationUser = await FindApplicationUserAsync(user, cancellationToken);

        return applicationUser is not null &&
            await userManager.CheckPasswordAsync(applicationUser, password);
    }

    public async Task<Result<UserGeneratedToken>> GenerateEmailConfirmationTokenAsync(
        User user,
        CancellationToken cancellationToken)
    {
        var applicationUser = await FindApplicationUserAsync(user, cancellationToken);

        if (applicationUser is null)
        {
            return UserNotFound<UserGeneratedToken>();
        }

        var token = await userManager.GenerateEmailConfirmationTokenAsync(applicationUser);

        return Result.Success(
            new UserGeneratedToken(
                token,
                timeProvider.GetUtcNow().Add(options.Value.EmailConfirmationTokenLifetime)));
    }

    public async Task<Result> ConfirmEmailAsync(
        User user,
        string confirmationToken,
        CancellationToken cancellationToken)
    {
        var applicationUser = await FindApplicationUserAsync(user, cancellationToken);

        if (applicationUser is null)
        {
            return UserNotFound();
        }

        var confirmEmailResult = await userManager.ConfirmEmailAsync(applicationUser, confirmationToken);

        return confirmEmailResult.Succeeded
            ? Result.Success()
            : IdentityResultMapper.ToResult(confirmEmailResult);
    }

    public async Task<Result<UserGeneratedToken>> GeneratePasswordResetTokenAsync(
        User user,
        CancellationToken cancellationToken)
    {
        var applicationUser = await FindApplicationUserAsync(user, cancellationToken);

        if (applicationUser is null)
        {
            return UserNotFound<UserGeneratedToken>();
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(applicationUser);

        return Result.Success(
            new UserGeneratedToken(
                token,
                timeProvider.GetUtcNow().Add(options.Value.PasswordResetTokenLifetime)));
    }

    public async Task<Result> ResetPasswordAsync(
        User user,
        string passwordResetToken,
        string newPassword,
        CancellationToken cancellationToken)
    {
        var applicationUser = await FindApplicationUserAsync(user, cancellationToken);

        if (applicationUser is null)
        {
            return UserNotFound();
        }

        var resetPasswordResult = await userManager.ResetPasswordAsync(
            applicationUser,
            passwordResetToken,
            newPassword);

        return resetPasswordResult.Succeeded
            ? Result.Success()
            : IdentityResultMapper.ToResult(resetPasswordResult);
    }

    private Task<ApplicationUser?> FindApplicationUserAsync(
        User user,
        CancellationToken cancellationToken)
    {
        return userManager.Users
            .SingleOrDefaultAsync(
                applicationUser => applicationUser.Id == user.Id.Value,
                cancellationToken);
    }

    private static Result<TValue> UserNotFound<TValue>()
    {
        return Result.Failure<TValue>(
            Error.NotFound("User was not found."));
    }

    private static Result UserNotFound()
    {
        return Result.Failure(
            Error.NotFound("User was not found."));
    }
}
