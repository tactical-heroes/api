using Microsoft.EntityFrameworkCore;

using OpenIddict.EntityFrameworkCore.Models;

using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Services;

public sealed class UserSessionService(
    IOpenIddictTokenManager tokenManager,
    IOpenIddictAuthorizationManager authorizationManager,
    IdentityWriteDbContext dbContext,
    ICurrentUserSessionRefresh? sessionRefresh = null) : IUserSessionService
{
    public async Task<Result> RevokeSessionAsync(
        Guid userId,
        string authorizationId,
        string clientId,
        CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty || !Guid.TryParse(authorizationId, out var id) || string.IsNullOrWhiteSpace(clientId))
        {
            return Result.Failure(Error.Unauthorized(message: "Current session is invalid."));
        }

        var subject = userId.ToString();
        var authorizations = dbContext.Set<OpenIddictEntityFrameworkCoreAuthorization<Guid>>()
            .Where(authorization => authorization.Id == id
                && authorization.Subject == subject
                && authorization.Application != null
                && authorization.Application.ClientId == clientId);
        if (!await authorizations.AnyAsync(cancellationToken))
        {
            return Result.Failure(Error.Unauthorized(message: "Current session is invalid."));
        }

        await authorizations.Where(authorization => authorization.Status != OpenIddictConstants.Statuses.Revoked)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(authorization => authorization.Status, OpenIddictConstants.Statuses.Revoked)
                    .SetProperty(authorization => authorization.ConcurrencyToken, _ => Guid.NewGuid().ToString()),
                cancellationToken);
        await dbContext.Set<OpenIddictEntityFrameworkCoreToken<Guid>>()
            .Where(token => token.Authorization != null && token.Authorization.Id == id)
            .Where(token => token.Status != OpenIddictConstants.Statuses.Revoked)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(token => token.Status, OpenIddictConstants.Statuses.Revoked)
                    .SetProperty(token => token.ConcurrencyToken, _ => Guid.NewGuid().ToString()),
                cancellationToken);

        return Result.Success();
    }

    internal async Task<Result<string?>> GetCurrentRoleAuthorizationIdAsync(
        Guid roleId,
        string? accessToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return Result.Success<string?>(value: null);
        }

        var token = await tokenManager.FindByReferenceIdAsync(accessToken, cancellationToken);
        if (token is null)
        {
            return InvalidSession();
        }

        if (await tokenManager.GetAuthorizationIdAsync(token, cancellationToken) is null)
        {
            return Result.Success<string?>(value: null);
        }

        if (!Guid.TryParse(await tokenManager.GetSubjectAsync(token, cancellationToken), out var userId))
        {
            return InvalidSession();
        }

        var result = await GetCurrentAuthorizationIdAsync(
            userId: userId,
            accessToken: accessToken,
            cancellationToken: cancellationToken);
        if (result.IsFailure)
        {
            return result;
        }

        var isMember = await GetRoleMemberIds(roleId).ContainsAsync(userId, cancellationToken);
        return Result.Success(isMember ? result.Value : null);
    }

    internal async Task<Result<string?>> GetCurrentAuthorizationIdAsync(
        Guid userId,
        string? accessToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return Result.Success<string?>(value: null);
        }

        var token = await tokenManager.FindByReferenceIdAsync(accessToken, cancellationToken);
        if (token is null
            || !await tokenManager.HasTypeAsync(
                token: token,
                type: OpenIddictConstants.TokenTypeIdentifiers.AccessToken,
                cancellationToken: cancellationToken)
            || !await tokenManager.HasStatusAsync(
                token: token,
                status: OpenIddictConstants.Statuses.Valid,
                cancellationToken: cancellationToken)
            || await tokenManager.GetSubjectAsync(token, cancellationToken) != userId.ToString())
        {
            return InvalidSession();
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
            || await authorizationManager.GetSubjectAsync(authorization, cancellationToken) != userId.ToString())
        {
            return InvalidSession();
        }

        return Result.Success(authorizationId);
    }

    internal async Task RevokeAsync(
        Guid userId,
        string? preservedAuthorizationId,
        CancellationToken cancellationToken)
    {
        var authorizations = await authorizationManager.FindBySubjectAsync(userId.ToString(), cancellationToken)
            .ToListAsync(cancellationToken);
        foreach (var authorization in authorizations)
        {
            if (preservedAuthorizationId is not null
                && await authorizationManager.GetIdAsync(authorization, cancellationToken) == preservedAuthorizationId)
            {
                continue;
            }

            if (!await authorizationManager.TryRevokeAsync(authorization, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                throw new InvalidOperationException("Failed to revoke a user authorization.");
            }
        }

        var tokens = await tokenManager.FindBySubjectAsync(userId.ToString(), cancellationToken)
            .ToListAsync(cancellationToken);
        foreach (var token in tokens)
        {
            if (preservedAuthorizationId is not null
                && await tokenManager.GetAuthorizationIdAsync(token, cancellationToken) == preservedAuthorizationId
                && await tokenManager.HasTypeAsync(
                    token: token,
                    type: OpenIddictConstants.TokenTypeIdentifiers.RefreshToken,
                    cancellationToken: cancellationToken))
            {
                continue;
            }

            if (!await tokenManager.TryRevokeAsync(token, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                throw new InvalidOperationException("Failed to revoke a user token.");
            }
        }

        if (preservedAuthorizationId is not null)
        {
            sessionRefresh?.RequireRefresh();
        }
    }

    internal async Task InvalidateAccessTokensAsync(
        Guid userId,
        bool refreshCurrentSession,
        CancellationToken cancellationToken)
    {
        var tokens = await tokenManager.FindBySubjectAsync(userId.ToString(), cancellationToken)
            .ToListAsync(cancellationToken);
        foreach (var token in tokens)
        {
            if (!await tokenManager.HasTypeAsync(
                token: token,
                type: OpenIddictConstants.TokenTypeIdentifiers.AccessToken,
                cancellationToken: cancellationToken)
                && !await tokenManager.HasTypeAsync(
                    token: token,
                    type: OpenIddictConstants.TokenTypeHints.AccessToken,
                    cancellationToken: cancellationToken))
            {
                continue;
            }

            if (!await tokenManager.TryRevokeAsync(token, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                throw new InvalidOperationException("Failed to revoke a user access token.");
            }
        }

        if (refreshCurrentSession)
        {
            sessionRefresh?.RequireRefresh();
        }
    }

    internal async Task InvalidateRoleMemberAccessTokensAsync(
        Guid roleId,
        bool refreshCurrentSession,
        CancellationToken cancellationToken)
    {
        var userIds = GetRoleMemberIds(roleId);
        var subjects = userIds.Select(id => id.ToString());
        await dbContext.Users.Where(user => userIds.Contains(user.Id))
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(user => user.SecurityStamp, _ => Guid.NewGuid().ToString())
                    .SetProperty(user => user.ConcurrencyStamp, _ => Guid.NewGuid().ToString()),
                cancellationToken);

        await dbContext.Set<OpenIddictEntityFrameworkCoreToken<Guid>>()
            .Where(token => subjects.Contains(token.Subject!))
            .Where(token => token.Type == OpenIddictConstants.TokenTypeIdentifiers.AccessToken
                || token.Type == OpenIddictConstants.TokenTypeHints.AccessToken)
            .Where(token => token.Status != OpenIddictConstants.Statuses.Revoked)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(token => token.Status, OpenIddictConstants.Statuses.Revoked)
                    .SetProperty(token => token.ConcurrencyToken, _ => Guid.NewGuid().ToString()),
                cancellationToken);

        if (refreshCurrentSession)
        {
            sessionRefresh?.RequireRefresh();
        }
    }

    private static Result<string?> InvalidSession()
    {
        return Result.Failure<string?>(Error.Unauthorized(message: "Current session is invalid."));
    }

    private IQueryable<Guid> GetRoleMemberIds(Guid roleId)
    {
        return dbContext.Set<ApplicationUserRole>()
            .Where(userRole => userRole.RoleId == roleId)
            .Select(userRole => userRole.UserId);
    }
}
