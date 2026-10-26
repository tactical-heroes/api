namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;

public interface IUserSessionService
{
    Task<Result> RevokeSessionAsync(
        Guid userId,
        string authorizationId,
        string clientId,
        CancellationToken cancellationToken);

    Task<Result<string?>> GetCurrentRoleAuthorizationIdAsync(
        Guid roleId,
        string? accessToken,
        CancellationToken cancellationToken);

    Task<Result<string?>> GetCurrentAuthorizationIdAsync(
        Guid userId,
        string? accessToken,
        CancellationToken cancellationToken);

    Task RevokeAsync(
        Guid userId,
        string? preservedAuthorizationId,
        CancellationToken cancellationToken);

    Task InvalidateAccessTokensAsync(
        Guid userId,
        string? currentAuthorizationId,
        CancellationToken cancellationToken);

    Task InvalidateRoleMemberAccessTokensAsync(
        Guid roleId,
        string? currentAuthorizationId,
        CancellationToken cancellationToken);

    Task<bool> IsAuthorizationValidAsync(Guid userId, string authorizationId, CancellationToken cancellationToken);
}
