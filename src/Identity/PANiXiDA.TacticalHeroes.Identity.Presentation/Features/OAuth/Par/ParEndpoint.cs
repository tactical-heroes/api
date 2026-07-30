using System.Net.Mime;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Par;

internal sealed class ParEndpoint : IEndpoint<OAuthEndpoints>
{
    public string Route { get; } = OAuthEndpointRoutes.PushedAuthorization;
    public string Name { get; } = "PushAuthorizationRequest";
    public string Summary { get; } = "Create pushed authorization request";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(handler: Handle)
            .AllowAnonymous()
            .Accepts<ParRequest>(contentType: MediaTypeNames.Application.FormUrlEncoded)
            .Produces<ParResponse>(statusCode: StatusCodes.Status201Created)
            .Produces<ParErrorResponse>(statusCode: StatusCodes.Status400BadRequest)
            .Produces<ParErrorResponse>(statusCode: StatusCodes.Status401Unauthorized);
    }

    private static ProblemHttpResult Handle()
    {
        return TypedResults.Problem(
            title: "OpenIddict pushed authorization endpoint was not handled.",
            detail: $"The {OAuthEndpointRoutes.GetPath(endpointRoute: OAuthEndpointRoutes.PushedAuthorization)} " +
                "route must be intercepted by the OpenIddict server pipeline. " +
                "If this fallback endpoint is executed, OpenIddict pushed authorization endpoint configuration is broken.",
            statusCode: StatusCodes.Status500InternalServerError);
    }
}
