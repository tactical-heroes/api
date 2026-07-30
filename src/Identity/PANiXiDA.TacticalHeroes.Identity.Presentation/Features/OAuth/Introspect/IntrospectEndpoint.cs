using System.Net.Mime;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Introspect;

internal sealed class IntrospectEndpoint : IEndpoint<OAuthEndpoints>
{
    public string Route { get; } = OAuthEndpointRoutes.Introspection;
    public string Name { get; } = "IntrospectToken";
    public string Summary { get; } = "Introspect access or refresh token";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(handler: Handle)
            .AllowAnonymous()
            .Accepts<IntrospectRequest>(contentType: MediaTypeNames.Application.FormUrlEncoded)
            .Produces<IntrospectResponse>(statusCode: StatusCodes.Status200OK)
            .Produces<IntrospectErrorResponse>(statusCode: StatusCodes.Status400BadRequest)
            .Produces<IntrospectErrorResponse>(statusCode: StatusCodes.Status401Unauthorized);
    }

    private static ProblemHttpResult Handle()
    {
        return TypedResults.Problem(
            title: "OpenIddict introspection endpoint was not handled.",
            detail: $"The {OAuthEndpointRoutes.GetPath(endpointRoute: OAuthEndpointRoutes.Introspection)} " +
                "route must be intercepted by the OpenIddict server pipeline. " +
                "If this fallback endpoint is executed, OpenIddict introspection endpoint configuration is broken.",
            statusCode: StatusCodes.Status500InternalServerError);
    }
}
