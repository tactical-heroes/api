using Microsoft.AspNetCore.Http;

using System.Security.Claims;

using PANiXiDA.TacticalHeroes.Identity.Application.Users;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Me;

internal sealed class GetCurrentUserEndpoint : IEndpoint<UsersEndpoints>
{
    public string Route { get; } = "/me";
    public string Name { get; } = "GetCurrentUser";
    public string Summary { get; } = "Get current identity user";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(Handle)
            .RequireAuthorization()
            .Produces<CurrentUserResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static IResult Handle(
        ClaimsPrincipal principal)
    {
        var userIdValue = principal.GetClaim(OpenIddictConstants.Claims.Subject);

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return TypedResults.Unauthorized();
        }

        var email = principal.GetClaim(OpenIddictConstants.Claims.Email);

        if (string.IsNullOrWhiteSpace(email))
        {
            return TypedResults.Unauthorized();
        }

        return TypedResults.Ok(
            new CurrentUserResponse(
                userId,
                email,
                GetRoles(principal),
                GetPermissions(principal)));
    }

    private static IReadOnlyCollection<string> GetPermissions(
        ClaimsPrincipal principal)
    {
        return principal.FindAll(AuthorizationClaimTypes.Permission)
            .Select(claim => claim.Value)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(permission => permission, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyCollection<string> GetRoles(
        ClaimsPrincipal principal)
    {
        return principal.FindAll(OpenIddictConstants.Claims.Role)
            .Select(claim => claim.Value)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(role => role, StringComparer.Ordinal)
            .ToArray();
    }
}
