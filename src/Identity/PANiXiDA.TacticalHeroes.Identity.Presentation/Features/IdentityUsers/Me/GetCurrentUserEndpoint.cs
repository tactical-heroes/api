using Microsoft.AspNetCore.Http;

using System.Security.Claims;

using PANiXiDA.TacticalHeroes.Identity.Application.IdentityUsers.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.IdentityUsers.Me;

internal sealed class GetCurrentUserEndpoint : IEndpoint<IdentityUsersEndpoints>
{
    public string Route { get; } = "/me";
    public string Name { get; } = "GetCurrentIdentityUser";
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
        IIdentityAuthenticationService identityAuthenticationService,
        CancellationToken cancellationToken)
    {
        var userIdValue = principal.GetClaim(OpenIddictConstants.Claims.Subject);

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return TypedResults.Unauthorized();
        }

        var result = await identityAuthenticationService.GetConfirmedUserAsync(
            userId,
            cancellationToken);

        return result.ToHttpResult(user =>
            TypedResults.Ok(
                new CurrentUserResponse(
                    user.Id,
                    user.Email,
                    user.Roles,
                    user.Permissions)));
    }
}
