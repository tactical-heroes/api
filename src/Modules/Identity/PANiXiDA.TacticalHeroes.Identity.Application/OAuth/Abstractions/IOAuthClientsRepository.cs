using PANiXiDA.TacticalHeroes.Modules.Identity.Application.OAuth.GetClientTokenPrincipal;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.OAuth.Abstractions;

public interface IOAuthClientsRepository
{
    Task<Result<OAuthClientTokenPrincipalReadModel>> GetTokenPrincipalByClientIdAsync(
        string clientId,
        CancellationToken cancellationToken);
}
