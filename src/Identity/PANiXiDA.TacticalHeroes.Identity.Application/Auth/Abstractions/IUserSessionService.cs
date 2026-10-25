namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;

public interface IUserSessionService
{
    Task<Result> RevokeSessionAsync(
        Guid userId,
        string authorizationId,
        string clientId,
        CancellationToken cancellationToken);
}
