using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.GetClientTokenPrincipal;

public sealed class GetClientTokenPrincipalHandler(IOAuthClientsRepository clientsRepository)
    : IQueryHandler<GetClientTokenPrincipalQuery, Result<OAuthClientTokenPrincipalReadModel>>
{
    public Task<Result<OAuthClientTokenPrincipalReadModel>> HandleAsync(
        GetClientTokenPrincipalQuery query,
        CancellationToken cancellationToken)
    {
        return clientsRepository.GetTokenPrincipalByClientIdAsync(
            query.ClientId,
            cancellationToken);
    }
}
