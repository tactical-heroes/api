using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Common;
using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Claims;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Mappers;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.EmailRequests;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.Mappers;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.Queries;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Services;

public sealed class UserCredentialsService(
    UserManager<ApplicationUser> userManager,
    IRolesReadRepository rolesReadRepository,
    IOpenIddictTokenManager tokenManager,
    IOpenIddictAuthorizationManager authorizationManager,
    UserEmailRequestService emailRequests,
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

        var confirmationResult = await emailRequests.RequestAsync(
            applicationUser: applicationUser,
            purpose: EmailRequestPurpose.Confirmation,
            user: user,
            cancellationToken: cancellationToken);

        return confirmationResult.IsSuccess
            ? Result.Success(value: applicationUser.Id)
            : Result.Failure<Guid>(errors: confirmationResult.Errors);
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

        var authorizationId = await GetValidCurrentAuthorizationIdAsync(
            userId: userId,
            accessToken: accessToken,
            cancellationToken: cancellationToken);
        if (authorizationId is null)
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

        await RevokeOtherSessionsAsync(
            userId: userId,
            authorizationId: authorizationId,
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

        return await emailRequests.RequestAsync(
            applicationUser: applicationUser,
            purpose: EmailRequestPurpose.Confirmation,
            user: null,
            cancellationToken: cancellationToken);
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

        return await emailRequests.RequestAsync(
            applicationUser: applicationUser,
            purpose: EmailRequestPurpose.PasswordReset,
            user: null,
            cancellationToken: cancellationToken);
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

    private async Task<string?> GetValidCurrentAuthorizationIdAsync(
        Guid userId,
        string accessToken,
        CancellationToken cancellationToken)
    {
        var token = await tokenManager.FindByReferenceIdAsync(accessToken, cancellationToken);
        if (token is null
            || !await tokenManager.HasTypeAsync(
                token: token,
                type: OpenIddictConstants.TokenTypeIdentifiers.AccessToken,
                cancellationToken: cancellationToken)
            || !await tokenManager.HasStatusAsync(
                token: token,
                status: OpenIddictConstants.Statuses.Valid,
                cancellationToken: cancellationToken))
        {
            return null;
        }

        var authorizationId = await tokenManager.GetAuthorizationIdAsync(token, cancellationToken);
        var authorization = authorizationId is null
            ? null
            : await authorizationManager.FindByIdAsync(authorizationId, cancellationToken);
        if (authorization is null
            || !await authorizationManager.HasStatusAsync(
                authorization: authorization,
                status: OpenIddictConstants.Statuses.Valid,
                cancellationToken: cancellationToken)
            || !string.Equals(
                await authorizationManager.GetSubjectAsync(authorization, cancellationToken),
                userId.ToString(),
                StringComparison.Ordinal))
        {
            return null;
        }

        return authorizationId;
    }

    private async Task RevokeOtherSessionsAsync(
        Guid userId,
        string authorizationId,
        CancellationToken cancellationToken)
    {
        var authorizations = await authorizationManager.FindBySubjectAsync(userId.ToString(), cancellationToken)
            .ToListAsync(cancellationToken);
        foreach (var authorization in authorizations)
        {
            if (!string.Equals(
                await authorizationManager.GetIdAsync(authorization, cancellationToken),
                authorizationId,
                StringComparison.Ordinal)
                && !await authorizationManager.TryRevokeAsync(authorization, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                throw new InvalidOperationException("Failed to revoke a user authorization.");
            }
        }

        var tokens = await tokenManager.FindBySubjectAsync(userId.ToString(), cancellationToken)
            .ToListAsync(cancellationToken);
        foreach (var token in tokens)
        {
            if (!string.Equals(
                await tokenManager.GetAuthorizationIdAsync(token, cancellationToken),
                authorizationId,
                StringComparison.Ordinal)
                && !await tokenManager.TryRevokeAsync(token, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                throw new InvalidOperationException("Failed to revoke a user token.");
            }
        }
    }
}
