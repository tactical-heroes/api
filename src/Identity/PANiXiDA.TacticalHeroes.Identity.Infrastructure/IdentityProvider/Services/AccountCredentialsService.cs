using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Login;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Claims;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Mappers;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.Mappers;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.Queries;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Services;

public sealed class AccountCredentialsService(
    UserManager<ApplicationUser> userManager,
    IOptions<IdentityProviderOptions> options,
    IAggregateTracker aggregateTracker,
    TimeProvider timeProvider)
    : IAccountCredentialsService
{
    public async Task<Result<Guid>> RegisterAsync(
        string email,
        string userName,
        string password,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var userResult = User.Register(email);
        var userNameResult = UserName.Create(userName);
        var validationResult = Result.Combine(userResult, userNameResult);

        if (validationResult.IsFailure)
        {
            return Result.Failure<Guid>(validationResult.Errors);
        }

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var applicationUser = new ApplicationUser
        {
            Id = userResult.Value.Id.Value,
            Email = userResult.Value.Email.Value,
            UserName = userNameResult.Value.Value,
            EmailConfirmed = false,
            Status = UserStatus.Active.Name,
            LockoutEnabled = true,
            CreatedAt = nowUtc,
            UpdatedAt = nowUtc
        };

        var identityResult = await userManager.CreateAsync(applicationUser, password);

        if (!identityResult.Succeeded)
        {
            return IdentityResultMapper.ToResult<Guid>(identityResult);
        }

        var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(applicationUser);
        var confirmationResult = userResult.Value.RequestAccountConfirmation(
            confirmationToken,
            timeProvider.GetUtcNow().Add(options.Value.EmailConfirmationTokenLifetime));

        if (confirmationResult.IsFailure)
        {
            return Result.Failure<Guid>(confirmationResult.Errors);
        }

        aggregateTracker.Track(userResult.Value);

        return Result.Success(applicationUser.Id);
    }

    public async Task<Result<AuthenticatedAccountReadModel>> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = userManager.NormalizeEmail(email);
        var applicationUser = await userManager.Users
            .WithAuthorizationGraph()
            .SingleOrDefaultAsync(
                user => user.NormalizedEmail == normalizedEmail,
                cancellationToken);

        if (applicationUser is null)
        {
            return InvalidCredentials();
        }

        if (IsBlocked(applicationUser))
        {
            return Result.Failure<AuthenticatedAccountReadModel>(
                Error.Forbidden("Account is blocked."));
        }

        if (await userManager.IsLockedOutAsync(applicationUser))
        {
            return Result.Failure<AuthenticatedAccountReadModel>(
                Error.Forbidden("Account is locked out."));
        }

        if (!await userManager.CheckPasswordAsync(applicationUser, password))
        {
            var failureResult = await userManager.AccessFailedAsync(applicationUser);

            if (!failureResult.Succeeded)
            {
                return IdentityResultMapper.ToResult<AuthenticatedAccountReadModel>(failureResult);
            }

            return InvalidCredentials();
        }

        var resetResult = await userManager.ResetAccessFailedCountAsync(applicationUser);

        if (!resetResult.Succeeded)
        {
            return IdentityResultMapper.ToResult<AuthenticatedAccountReadModel>(resetResult);
        }

        if (!applicationUser.EmailConfirmed)
        {
            return Result.Failure<AuthenticatedAccountReadModel>(
                Error.Forbidden("Account is not confirmed."));
        }

        var claims = IdentityClaimsFactory.Create(
            applicationUser,
            userManager.Options);

        return Result.Success(
            new AuthenticatedAccountReadModel(
                applicationUser.Id,
                applicationUser.Email!,
                applicationUser.UserName!,
                claims));
    }

    public async Task<Result> ChangePasswordAsync(
        Guid accountId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var applicationUser = await userManager.FindByIdAsync(accountId.ToString());

        if (applicationUser is null)
        {
            return AccountNotFound();
        }

        if (IsBlocked(applicationUser))
        {
            return Result.Failure(Error.Forbidden("Account is blocked."));
        }

        if (await userManager.IsLockedOutAsync(applicationUser))
        {
            return Result.Failure(Error.Forbidden("Account is locked out."));
        }

        var result = await userManager.ChangePasswordAsync(
            applicationUser,
            currentPassword,
            newPassword);

        return IdentityResultMapper.ToResult(result);
    }

    public async Task<Result> ConfirmAsync(
        Guid accountId,
        string emailConfirmationToken,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var applicationUser = await userManager.FindByIdAsync(accountId.ToString());

        if (applicationUser is null)
        {
            return AccountNotFound();
        }

        if (applicationUser.EmailConfirmed)
        {
            return Result.Success();
        }

        var userResult = ApplicationUserMapper.ToDomain(applicationUser);

        if (userResult.IsFailure)
        {
            return Result.Failure(userResult.Errors);
        }

        var identityResult = await userManager.ConfirmEmailAsync(
            applicationUser,
            emailConfirmationToken);

        if (!identityResult.Succeeded)
        {
            return IdentityResultMapper.ToResult(identityResult);
        }

        var confirmResult = userResult.Value.ConfirmRegistration();

        if (confirmResult.IsFailure)
        {
            return confirmResult;
        }

        aggregateTracker.Track(userResult.Value);

        return Result.Success();
    }

    public async Task<Result> ResendConfirmationEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var applicationUser = await userManager.FindByEmailAsync(email);

        if (applicationUser is null ||
            applicationUser.EmailConfirmed ||
            IsBlocked(applicationUser))
        {
            return Result.Success();
        }

        var userResult = ApplicationUserMapper.ToDomain(applicationUser);

        if (userResult.IsFailure)
        {
            return Result.Failure(userResult.Errors);
        }

        var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(applicationUser);
        var confirmationResult = userResult.Value.RequestAccountConfirmation(
            confirmationToken,
            timeProvider.GetUtcNow().Add(options.Value.EmailConfirmationTokenLifetime));

        if (confirmationResult.IsFailure)
        {
            return confirmationResult;
        }

        aggregateTracker.Track(userResult.Value);

        return Result.Success();
    }

    public async Task<Result> ForgotPasswordAsync(
        string email,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var applicationUser = await userManager.FindByEmailAsync(email);

        if (applicationUser is null ||
            !applicationUser.EmailConfirmed ||
            IsBlocked(applicationUser))
        {
            return Result.Success();
        }

        var userResult = ApplicationUserMapper.ToDomain(applicationUser);

        if (userResult.IsFailure)
        {
            return Result.Failure(userResult.Errors);
        }

        var resetToken = await userManager.GeneratePasswordResetTokenAsync(applicationUser);
        var requestResult = userResult.Value.RequestPasswordReset(
            resetToken,
            timeProvider.GetUtcNow().Add(options.Value.PasswordResetTokenLifetime));

        if (requestResult.IsFailure)
        {
            return requestResult;
        }

        aggregateTracker.Track(userResult.Value);

        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(
        Guid accountId,
        string passwordResetToken,
        string newPassword,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var applicationUser = await userManager.FindByIdAsync(accountId.ToString());

        if (applicationUser is null)
        {
            return AccountNotFound();
        }

        if (!applicationUser.EmailConfirmed)
        {
            return Result.Failure(Error.Forbidden("Account is not confirmed."));
        }

        if (IsBlocked(applicationUser))
        {
            return Result.Failure(Error.Forbidden("Account is blocked."));
        }

        var result = await userManager.ResetPasswordAsync(
            applicationUser,
            passwordResetToken,
            newPassword);

        return IdentityResultMapper.ToResult(result);
    }

    private static bool IsBlocked(ApplicationUser applicationUser)
    {
        return string.Equals(
            applicationUser.Status,
            UserStatus.Blocked.Name,
            StringComparison.Ordinal);
    }

    private static Result<AuthenticatedAccountReadModel> InvalidCredentials()
    {
        return Result.Failure<AuthenticatedAccountReadModel>(
            Error.Unauthorized("Invalid email or password."));
    }

    private static Result AccountNotFound()
    {
        return Result.Failure(Error.NotFound("Account was not found."));
    }
}
