using PANiXiDA.TacticalHeroes.Identity.Application.Clients.GetClientTokenPrincipal;

namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Abstractions;

public interface IOAuthClientsRepository
{
    Task<Result<OAuthClientTokenPrincipalReadModel>> GetTokenPrincipalByClientIdAsync(
        string clientId,
        CancellationToken cancellationToken);
}
