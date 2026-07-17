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
                Error.NotFound("OAuth client was not found."));
        }

        var displayName = await applicationManager.GetDisplayNameAsync(
            application,
            cancellationToken);
        IReadOnlyCollection<Claim> claims =
        [
            new(OpenIddictConstants.Claims.Subject, clientId),
            new(OpenIddictConstants.Claims.ClientId, clientId),
            new(OpenIddictConstants.Claims.Name, displayName ?? clientId)
        ];

        return Result.Success(
            new OAuthClientTokenPrincipalReadModel(claims));
    }
}
