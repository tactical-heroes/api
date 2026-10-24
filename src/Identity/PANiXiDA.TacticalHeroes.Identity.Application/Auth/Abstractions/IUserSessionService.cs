namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;

public interface IUserSessionService
{
    Task<Result<string?>> GetCurrentAuthorizationIdAsync(
        Guid userId,
        string? accessToken,
        CancellationToken cancellationToken);

    Task<bool> IsAuthorizationValidAsync(Guid userId, string authorizationId, CancellationToken cancellationToken);
}
