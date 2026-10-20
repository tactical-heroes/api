using System.Security.Claims;

using Microsoft.AspNetCore.Http;

using OpenIddict.Abstractions;

using PANiXiDA.Core.ResultPattern;
using PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;
using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.GetClientTokenPrincipal;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.OAuth;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation;

internal sealed class PermissionTestClientsRepository(
    IOpenIddictApplicationManager manager,
    IHttpContextAccessor accessor) : IOAuthClientsRepository
{
    internal const string PermissionsHeader = "X-Test-Permissions";

    public async Task<Result<OAuthClientTokenPrincipalReadModel>> GetTokenPrincipalByClientIdAsync(
        string clientId,
        CancellationToken cancellationToken)
    {
        var result = await new OAuthClientsRepository(manager)
            .GetTokenPrincipalByClientIdAsync(clientId, cancellationToken);
        if (result.IsFailure)
        {
            return result;
        }

        var request = accessor.HttpContext!.Request;
        var permissions = request.Headers.TryGetValue(PermissionsHeader, out var values)
            ? values.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries)
            : Permissions.All.Select(permission => permission.Code).ToArray();

        IReadOnlyCollection<Claim> claims =
        [
            .. result.Value.Claims,
            .. permissions.Select(permission => new Claim("permission", permission))
        ];

        return Result.Success(new OAuthClientTokenPrincipalReadModel(Claims: claims));
    }
}
