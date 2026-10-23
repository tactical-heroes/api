namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Services;

public sealed class UserSessionService(
    IOpenIddictTokenManager tokenManager,
    IOpenIddictAuthorizationManager authorizationManager)
{
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
    }

    private static Result<string?> InvalidSession()
    {
        return Result.Failure<string?>(Error.Unauthorized(message: "Current session is invalid."));
    }
}
