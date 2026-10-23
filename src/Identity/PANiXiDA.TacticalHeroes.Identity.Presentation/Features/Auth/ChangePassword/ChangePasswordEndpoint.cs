using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

using PANiXiDA.Core.Application.Authentication.Abstractions;

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
        ICurrentUser currentUser,
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return TypedResults.Unauthorized();
        }

        var cookie = await httpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        var result = await mediator.SendAsync(
            ChangePasswordMapper.ToCommand(
                request: request,
                userId: userId),
            cancellationToken);

        if (result.IsFailure)
        {
            return result.ToHttpProblem();
        }

        if (cookie.Succeeded
            && Guid.TryParse(
                cookie.Principal?.GetClaim(OpenIddictConstants.Claims.Subject),
                out var cookieUserId)
            && cookieUserId == userId)
        {
            await httpContext.SignInAsync(
                scheme: IdentityConstants.ApplicationScheme,
                principal: AuthenticatedUserPrincipalFactory.Create(result.Value),
                properties: cookie.Properties);
        }

        return TypedResults.NoContent();
    }
}
