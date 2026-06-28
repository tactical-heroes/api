using Microsoft.AspNetCore.Http;

using System.Security.Claims;

using PANiXiDA.TacticalHeroes.Identity.Application.Users;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetAuthenticated;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Me;

internal sealed class GetCurrentUserEndpoint : IEndpoint<UsersEndpoints>
{
    public string Route { get; } = "/me";
    public string Name { get; } = "GetCurrentUser";
    public string Summary { get; } = "Get current identity user";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(HandleAsync)
            .RequireAuthorization()
            .Produces<CurrentUserResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        ClaimsPrincipal principal,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userIdValue = principal.GetClaim(OpenIddictConstants.Claims.Subject);

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return TypedResults.Unauthorized();
        }

        var result = await mediator.QueryAsync(
            new GetAuthenticatedUserQuery(userId),
            cancellationToken);

        return result.ToHttpResult(user =>
            TypedResults.Ok(
                new CurrentUserResponse(
                    user.Id,
                    user.Email,
                    user.Roles,
                    GetPermissions(user.Claims))));
    }

    private static IReadOnlyCollection<string> GetPermissions(
        IReadOnlyCollection<AuthorizationClaim> claims)
    {
        return claims
            .Where(claim => claim.Type == AuthorizationClaimTypes.Permission)
            .Select(claim => claim.Value)
            .ToArray();
    }
}
