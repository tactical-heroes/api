using System.Security.Claims;

using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.GetClientTokenPrincipal;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.OAuth;

public sealed class OAuthClientsRepository(IOpenIddictApplicationManager applicationManager)
    : IOAuthClientsRepository
{
    public async Task<Result<OAuthClientTokenPrincipalReadModel>> GetTokenPrincipalByClientIdAsync(
        string clientId,
        CancellationToken cancellationToken)
    {
        var application = await applicationManager.FindByClientIdAsync(
            clientId,
            cancellationToken);

        if (application is null)
        {
            return Result.Failure<OAuthClientTokenPrincipalReadModel>(
                error: Error.NotFound(message: "OAuth client was not found."));
        }

        var displayName = await applicationManager.GetDisplayNameAsync(
            application,
            cancellationToken);
        IReadOnlyCollection<Claim> claims =
        [
            new(type: OpenIddictConstants.Claims.Subject, value: clientId),
            new(type: OpenIddictConstants.Claims.ClientId, value: clientId),
            new(type: OpenIddictConstants.Claims.Name, value: displayName ?? clientId)
        ];

        return Result.Success(
            value: new OAuthClientTokenPrincipalReadModel(Claims: claims));
    }
}
