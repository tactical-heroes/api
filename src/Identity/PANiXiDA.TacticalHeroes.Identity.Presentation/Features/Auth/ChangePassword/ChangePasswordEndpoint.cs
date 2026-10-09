using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.ChangePassword;

internal sealed class ChangePasswordEndpoint : IEndpoint<AuthEndpoints>
{
    public string Route { get; } = "/change-password";
    public string Name { get; } = "ChangePassword";
    public string Summary { get; } = "Change current user password";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(builder.Route, HandleAsync)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        ChangePasswordRequest request,
        ClaimsPrincipal user,
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userIdValue = user.FindFirst(OpenIddictConstants.Claims.Subject)?.Value;

        var authorizationId = user.GetAuthorizationId();

        if (!Guid.TryParse(input: userIdValue, result: out var userId) ||
            string.IsNullOrWhiteSpace(authorizationId))
        {
            return TypedResults.Unauthorized();
        }

        var cookie = await httpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        var result = await mediator.SendAsync(
            ChangePasswordMapper.ToCommand(
                request: request,
                userId: userId,
                authorizationId: authorizationId),
            cancellationToken);

        if (result.IsFailure)
        {
            return result.ToHttpProblem();
        }

        if (cookie.Succeeded &&
            string.Equals(cookie.Principal?.GetClaim(OpenIddictConstants.Claims.Subject), userIdValue, StringComparison.Ordinal))
        {
            await httpContext.SignInAsync(
                scheme: IdentityConstants.ApplicationScheme,
                principal: AuthenticatedUserPrincipalFactory.Create(result.Value),
                properties: cookie.Properties);
        }

        return TypedResults.NoContent();
    }
}
