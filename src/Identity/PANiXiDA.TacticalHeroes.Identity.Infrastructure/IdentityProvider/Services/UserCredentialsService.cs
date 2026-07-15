using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Mappers;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.Mappers;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Services;

public sealed class UserCredentialsService(
    UserManager<ApplicationUser> userManager,
    IUsersRepository usersRepository,
    IOptions<IdentityProviderOptions> options,
    IAggregateTracker aggregateTracker,
    TimeProvider timeProvider)
    : IUserCredentialsService
{
    public async Task<Result<Guid>> RegisterAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        var existingUser = await usersRepository.GetByEmailAsync(email, cancellationToken);

        if (existingUser is not null)
        {
            return Result.Failure<Guid>(
                Error.Conflict("User with this email already exists."));
        }

        var userResult = User.Register(email);

        if (userResult.IsFailure)
        {
            return Result.Failure<Guid>(userResult.Errors);
        }

        var addUserResult = await usersRepository.AddAsync(
            userResult.Value,
            password,
            cancellationToken);

        if (addUserResult.IsFailure)
        {
            return Result.Failure<Guid>(addUserResult.Errors);
        }

        var applicationUserResult = await GetApplicationUserAsync(
            userResult.Value.Id.Value,
            cancellationToken);

        if (applicationUserResult.IsFailure)
        {
            return Result.Failure<Guid>(applicationUserResult.Errors);
        }

        var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(applicationUserResult.Value);
        var requestConfirmationResult = userResult.Value.RequestAccountConfirmation(
            confirmationToken,
            timeProvider.GetUtcNow().Add(options.Value.EmailConfirmationTokenLifetime));

        if (requestConfirmationResult.IsFailure)
        {
            return Result.Failure<Guid>(requestConfirmationResult.Errors);
        }

        return Result.Success(userResult.Value.Id.Value);
    }

    public async Task<Result<Guid>> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        var applicationUser = await userManager.FindByEmailAsync(email);

        if (applicationUser is null ||
            !await userManager.CheckPasswordAsync(applicationUser, password))
        {
            return Result.Failure<Guid>(
                Error.Unauthorized("Invalid credentials."));
        }

        if (!applicationUser.EmailConfirmed)
        {
            return Result.Failure<Guid>(
                Error.Forbidden("Account is not confirmed."));
        }

        return Result.Success(applicationUser.Id);
    }

    public async Task<Result> ConfirmRegistrationAsync(
        Guid userId,
        string confirmationToken,
        CancellationToken cancellationToken)
    {
        var applicationUserResult = await GetApplicationUserAsync(
            userId,
            cancellationToken);

        if (applicationUserResult.IsFailure)
        {
            return applicationUserResult;
        }

        var userResult = ApplicationUserMapper.ToDomain(applicationUserResult.Value);

        if (userResult.IsFailure)
        {
            return Result.Failure(userResult.Errors);
        }

        var confirmEmailResult = await userManager.ConfirmEmailAsync(
            applicationUserResult.Value,
            confirmationToken);

        if (!confirmEmailResult.Succeeded)
        {
            return IdentityResultMapper.ToResult(confirmEmailResult);
        }

        var confirmRegistrationResult = userResult.Value.ConfirmRegistration();

        if (confirmRegistrationResult.IsFailure)
        {
            return confirmRegistrationResult;
        }

        aggregateTracker.Track(userResult.Value);

        return Result.Success();
    }

    public async Task<Result> RequestPasswordResetAsync(
        string email,
        CancellationToken cancellationToken)
    {
        var applicationUser = await GetApplicationUserByEmailAsync(
            email,
            cancellationToken);

        if (applicationUser is null || !applicationUser.EmailConfirmed)
        {
            return Result.Success();
        }

        var userResult = ApplicationUserMapper.ToDomain(applicationUser);

        if (userResult.IsFailure)
        {
            return Result.Failure(userResult.Errors);
        }

        var passwordResetToken = await userManager.GeneratePasswordResetTokenAsync(applicationUser);
        var result = userResult.Value.RequestPasswordReset(
            passwordResetToken,
            timeProvider.GetUtcNow().Add(options.Value.PasswordResetTokenLifetime));

        if (result.IsFailure)
        {
            return result;
        }

        aggregateTracker.Track(userResult.Value);

        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(
        Guid userId,
        string passwordResetToken,
        string newPassword,
        CancellationToken cancellationToken)
    {
        var applicationUserResult = await GetApplicationUserAsync(
            userId,
            cancellationToken);

        if (applicationUserResult.IsFailure)
        {
            return applicationUserResult;
        }

        var resetPasswordResult = await userManager.ResetPasswordAsync(
            applicationUserResult.Value,
            passwordResetToken,
            newPassword);

        return resetPasswordResult.Succeeded
            ? Result.Success()
            : IdentityResultMapper.ToResult(resetPasswordResult);
    }

    private async Task<Result<ApplicationUser>> GetApplicationUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var applicationUser = await QueryUsers()
            .SingleOrDefaultAsync(
                applicationUser => applicationUser.Id == userId,
                cancellationToken);

        return applicationUser is null
            ? UserNotFound<ApplicationUser>()
            : Result.Success(applicationUser);
    }

    private IQueryable<ApplicationUser> QueryUsers()
    {
        return userManager.Users
            .Include(applicationUser => applicationUser.Roles)
            .Include(applicationUser => applicationUser.Claims);
    }

    private async Task<ApplicationUser?> GetApplicationUserByEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = userManager.NormalizeEmail(email);

        return await QueryUsers()
            .SingleOrDefaultAsync(
                applicationUser => applicationUser.NormalizedEmail == normalizedEmail,
                cancellationToken);
    }

    private static Result<TValue> UserNotFound<TValue>()
    {
        return Result.Failure<TValue>(
            Error.NotFound("User was not found."));
    }
}
