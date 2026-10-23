using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Common;
using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Claims;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Mappers;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.IdentityProvider;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.Mappers;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.Queries;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Services;

public sealed class UserCredentialsService(
    UserManager<ApplicationUser> userManager,
    IRolesReadRepository rolesReadRepository,
    IOpenIddictTokenManager tokenManager,
    UserSessionService userSessionService,
    IOptions<IdentityProviderOptions> options,
    IAggregateTracker aggregateTracker,
    TimeProvider timeProvider)
    : IUserCredentialsService
{
    public async Task<Result<Guid>> RegisterAsync(
        string email,
        string userName,
        string password,
        CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(value: email);
        var userNameResult = UserName.Create(value: userName);
        var validationResult = Result.Combine(emailResult, userNameResult);

        if (validationResult.IsFailure)
        {
            return Result.Failure<Guid>(errors: validationResult.Errors);
        }

        var defaultRoleId = await rolesReadRepository.GetDefaultIdAsync(cancellationToken);
        if (defaultRoleId is null)
        {
            return Result.Failure<Guid>(
                error: Error.NotFound(message: "Default role was not found."));
        }

        var roleIdResult = RoleId.Create(value: defaultRoleId.Value);
        if (roleIdResult.IsFailure)
        {
            return Result.Failure<Guid>(errors: roleIdResult.Errors);
        }

        var user = User.Register(
            email: emailResult.Value,
            userName: userNameResult.Value,
            roleId: roleIdResult.Value);
        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var applicationUser = ApplicationUserMapper.ToDbModel(
            user: user,
            createdAt: nowUtc,
            updatedAt: nowUtc);

        var identityResult = await userManager.CreateAsync(user: applicationUser, password: password);

        if (!identityResult.Succeeded)
        {
            return IdentityResultMapper.ToResult<Guid>(result: identityResult);
        }

        var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(applicationUser);
        var tokenResult = UserActionToken.Create(
            value: confirmationToken,
            expiresAtUtc: timeProvider.GetUtcNow().Add(options.Value.EmailConfirmationTokenLifetime));

        if (tokenResult.IsFailure)
        {
            return Result.Failure<Guid>(errors: tokenResult.Errors);
        }

        var confirmationResult = user.RequestEmailConfirmation(tokenResult.Value);

        if (confirmationResult.IsFailure)
        {
            return Result.Failure<Guid>(errors: confirmationResult.Errors);
        }

        aggregateTracker.Track(user);

        return Result.Success(value: applicationUser.Id);
    }

    public async Task<Result<AuthenticatedUserReadModel>> LoginAsync(
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
            return Result.Failure<AuthenticatedUserReadModel>(
                error: Error.Forbidden(message: "User is blocked."));
        }

        if (await userManager.IsLockedOutAsync(applicationUser))
        {
            return Result.Failure<AuthenticatedUserReadModel>(
                error: Error.Forbidden(message: "User is locked out."));
        }

        if (!await userManager.CheckPasswordAsync(applicationUser, password))
        {
            var failureResult = await userManager.AccessFailedAsync(applicationUser);

            if (!failureResult.Succeeded)
            {
                return IdentityResultMapper.ToResult<AuthenticatedUserReadModel>(result: failureResult);
            }

            return InvalidCredentials();
        }

        var resetResult = await userManager.ResetAccessFailedCountAsync(applicationUser);

        if (!resetResult.Succeeded)
        {
            return IdentityResultMapper.ToResult<AuthenticatedUserReadModel>(result: resetResult);
        }

        if (!applicationUser.EmailConfirmed)
        {
            return Result.Failure<AuthenticatedUserReadModel>(
                error: Error.Forbidden(message: "User is not confirmed."));
        }

        return Result.Success(value: CreateAuthenticatedUserReadModel(applicationUser));
    }

    public async Task<Result<AuthenticatedUserReadModel>> ChangePasswordAsync(
        Guid userId,
        string accessToken,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken)
    {
        var applicationUser = await userManager.Users
            .WithAuthorizationGraph()
            .SingleOrDefaultAsync(user => user.Id == userId, cancellationToken);

        if (applicationUser is null)
        {
            return Result.Failure<AuthenticatedUserReadModel>(
                error: Error.NotFound(message: "User was not found."));
        }

        if (IsBlocked(applicationUser))
        {
            return Result.Failure<AuthenticatedUserReadModel>(
                error: Error.Forbidden(message: "User is blocked."));
        }

        if (await userManager.IsLockedOutAsync(applicationUser))
        {
            return Result.Failure<AuthenticatedUserReadModel>(
                error: Error.Forbidden(message: "User is locked out."));
        }

        var sessionResult = await userSessionService.GetCurrentAuthorizationIdAsync(
            userId: userId,
            accessToken: accessToken,
            cancellationToken: cancellationToken);
        if (sessionResult.IsFailure || sessionResult.Value is null)
        {
            return Result.Failure<AuthenticatedUserReadModel>(
                error: Error.Unauthorized(message: "Current session is invalid."));
        }

        var result = await userManager.ChangePasswordAsync(
            user: applicationUser,
            currentPassword: currentPassword,
            newPassword: newPassword);

        if (!result.Succeeded)
        {
            return IdentityResultMapper.ToResult<AuthenticatedUserReadModel>(result);
        }

        await userSessionService.RevokeAsync(
            userId: userId,
            preservedAuthorizationId: sessionResult.Value,
            cancellationToken: cancellationToken);

        return Result.Success(value: CreateAuthenticatedUserReadModel(applicationUser));
    }

    public async Task<Result> ConfirmEmailAsync(
        Guid userId,
        string emailConfirmationToken,
        CancellationToken cancellationToken)
    {
        var applicationUser = await userManager.FindByIdAsync(userId.ToString());

        if (applicationUser is null)
        {
            return UserNotFound();
        }

        if (applicationUser.EmailConfirmed)
        {
            return Result.Success();
        }

        var userResult = ApplicationUserMapper.ToDomain(user: applicationUser);

        if (userResult.IsFailure)
        {
            return Result.Failure(errors: userResult.Errors);
        }

        var identityResult = await userManager.ConfirmEmailAsync(
            applicationUser,
            emailConfirmationToken);

        if (!identityResult.Succeeded)
        {
            return IdentityResultMapper.ToResult(result: identityResult);
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
        var applicationUser = await userManager.FindByEmailAsync(email);

        if (applicationUser is null
            || applicationUser.EmailConfirmed
            || IsBlocked(applicationUser))
        {
            return Result.Success();
        }

        var userResult = ApplicationUserMapper.ToDomain(user: applicationUser);

        if (userResult.IsFailure)
        {
            return Result.Failure(errors: userResult.Errors);
        }

        var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(applicationUser);
        var tokenResult = UserActionToken.Create(
            value: confirmationToken,
            expiresAtUtc: timeProvider.GetUtcNow().Add(options.Value.EmailConfirmationTokenLifetime));

        if (tokenResult.IsFailure)
        {
            return Result.Failure(errors: tokenResult.Errors);
        }

        var confirmationResult = userResult.Value.RequestEmailConfirmation(tokenResult.Value);

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
        var applicationUser = await userManager.FindByEmailAsync(email);

        if (applicationUser is null
            || !applicationUser.EmailConfirmed
            || IsBlocked(applicationUser))
        {
            return Result.Success();
        }

        var userResult = ApplicationUserMapper.ToDomain(user: applicationUser);

        if (userResult.IsFailure)
        {
            return Result.Failure(errors: userResult.Errors);
        }

        var resetToken = await userManager.GeneratePasswordResetTokenAsync(applicationUser);
        var tokenResult = UserActionToken.Create(
            value: resetToken,
            expiresAtUtc: timeProvider.GetUtcNow().Add(options.Value.AccountRecoveryTokenLifetime));

        if (tokenResult.IsFailure)
        {
            return Result.Failure(errors: tokenResult.Errors);
        }

        var requestResult = userResult.Value.RequestPasswordReset(tokenResult.Value);

        if (requestResult.IsFailure)
        {
            return requestResult;
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
        var applicationUser = await userManager.FindByIdAsync(userId.ToString());

        if (applicationUser is null)
        {
            return UserNotFound();
        }

        if (!applicationUser.EmailConfirmed)
        {
            return Result.Failure(error: Error.Forbidden(message: "User is not confirmed."));
        }

        if (IsBlocked(applicationUser))
        {
            return Result.Failure(error: Error.Forbidden(message: "User is blocked."));
        }

        var result = await userManager.ResetPasswordAsync(
            user: applicationUser,
            token: passwordResetToken,
            newPassword: newPassword);

        if (result.Succeeded)
        {
            await tokenManager.RevokeBySubjectAsync(userId.ToString(), cancellationToken);
        }

        return IdentityResultMapper.ToResult(result: result);
    }

    private static Result<AuthenticatedUserReadModel> InvalidCredentials()
    {
        return Result.Failure<AuthenticatedUserReadModel>(
            error: Error.Unauthorized(message: "Invalid email or password."));
    }

    private static bool IsBlocked(ApplicationUser applicationUser)
    {
        return string.Equals(
            applicationUser.Status,
            UserStatus.Blocked.Name,
            StringComparison.Ordinal);
    }

    private static Result UserNotFound()
    {
        return Result.Failure(error: Error.NotFound(message: "User was not found."));
    }

    private AuthenticatedUserReadModel CreateAuthenticatedUserReadModel(ApplicationUser applicationUser)
    {
        return new AuthenticatedUserReadModel(
            Id: applicationUser.Id,
            Email: applicationUser.Email!,
            UserName: applicationUser.UserName!,
            Claims: IdentityClaimsFactory.Create(applicationUser, userManager.Options));
    }
}
