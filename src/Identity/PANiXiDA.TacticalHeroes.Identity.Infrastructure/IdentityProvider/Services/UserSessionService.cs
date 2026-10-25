using Microsoft.AspNetCore.Http;
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
    ICurrentUserSessionRefresh? sessionRefresh = null,
    IHttpContextAccessor? httpContextAccessor = null) : IUserSessionService
{
    private CurrentSession? _currentSession;

    private CurrentSession? CachedSession
    {
        get => httpContextAccessor?.HttpContext is { } context
            ? context.Features.Get<CurrentSession>()
            : _currentSession;

        set
        {
            if (httpContextAccessor?.HttpContext is { } context)
            {
                context.Features.Set(value);
            }
            else
            {
                _currentSession = value;
            }
        }
    }

    public async Task<Result> RevokeSessionAsync(
        Guid userId,
        string authorizationId,
        string clientId,
        CancellationToken cancellationToken)
    {
        CachedSession = null;
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

    public async Task<Result<string?>> GetCurrentRoleAuthorizationIdAsync(
        Guid roleId,
        string? accessToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return Result.Success<string?>(value: null);
        }

        var session = await GetCurrentSessionAsync(accessToken, cancellationToken);
        if (session is null)
        {
            return InvalidSession();
        }

        if (session.AuthorizationId is null)
        {
            return Result.Success<string?>(value: null);
        }

        var isMember = await GetRoleMemberIds(roleId).ContainsAsync(session.UserId, cancellationToken);
        return Result.Success(isMember ? session.AuthorizationId : null);
    }

    public async Task<Result<string?>> GetCurrentAuthorizationIdAsync(
        Guid userId,
        string? accessToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return Result.Success<string?>(value: null);
        }

        var session = await GetCurrentSessionAsync(accessToken, cancellationToken);
        return session is { AuthorizationId: not null } && session.UserId == userId
            ? Result.Success<string?>(session.AuthorizationId)
            : InvalidSession();
    }

    public async Task RevokeAsync(
        Guid userId,
        string? preservedAuthorizationId,
        CancellationToken cancellationToken)
    {
        CachedSession = null;
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

    public async Task InvalidateAccessTokensAsync(
        Guid userId,
        bool refreshCurrentSession,
        CancellationToken cancellationToken)
    {
        CachedSession = null;
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

    public async Task InvalidateRoleMemberAccessTokensAsync(
        Guid roleId,
        bool refreshCurrentSession,
        CancellationToken cancellationToken)
    {
        CachedSession = null;
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

    public Task<bool> IsAuthorizationValidAsync(
        Guid userId,
        string authorizationId,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(authorizationId, out var id))
        {
            return Task.FromResult(result: false);
        }

        var subject = userId.ToString();
        return dbContext.Set<OpenIddictEntityFrameworkCoreAuthorization<Guid>>()
            .AsNoTracking()
            .AnyAsync(
                authorization => authorization.Id == id
                    && authorization.Subject == subject
                    && authorization.Status == OpenIddictConstants.Statuses.Valid,
                cancellationToken);
    }

    private static Result<string?> InvalidSession()
    {
        return Result.Failure<string?>(Error.Unauthorized(message: "Current session is invalid."));
    }

    private async Task<CurrentSession?> GetCurrentSessionAsync(string accessToken, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (CachedSession is { } currentSession
            && string.Equals(currentSession.AccessToken, accessToken, StringComparison.Ordinal))
        {
            return currentSession;
        }

        var token = await tokenManager.FindByReferenceIdAsync(accessToken, cancellationToken);
        if (token is null)
        {
            return null;
        }

        var authorizationId = await tokenManager.GetAuthorizationIdAsync(token, cancellationToken);
        if (authorizationId is null)
        {
            return new CurrentSession(AccessToken: accessToken, UserId: Guid.Empty, AuthorizationId: null);
        }

        var subject = await tokenManager.GetSubjectAsync(token, cancellationToken);
        if (!Guid.TryParse(subject, out var userId)
            || !string.Equals(subject, userId.ToString(), StringComparison.Ordinal)
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

        var authorization = await authorizationManager.FindByIdAsync(authorizationId, cancellationToken);
        if (authorization is null
            || !await authorizationManager.HasStatusAsync(
                authorization: authorization,
                status: OpenIddictConstants.Statuses.Valid,
                cancellationToken: cancellationToken)
            || await authorizationManager.GetSubjectAsync(authorization, cancellationToken) != subject)
        {
            return null;
        }

        var session = new CurrentSession(
            AccessToken: accessToken, UserId: userId, AuthorizationId: authorizationId);
        CachedSession = session;
        return session;
    }

    private IQueryable<Guid> GetRoleMemberIds(Guid roleId)
    {
        return dbContext.Set<ApplicationUserRole>()
            .Where(userRole => userRole.RoleId == roleId)
            .Select(userRole => userRole.UserId);
    }

    private sealed record CurrentSession(string AccessToken, Guid UserId, string? AuthorizationId);
}
