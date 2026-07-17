using System.Net.Mime;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Revoke;

internal sealed class RevokeEndpoint : IEndpoint<OAuthEndpoints>
{
    public string Route { get; } = "/revoke";
    public string Name { get; } = "RevokeToken";
    public string Summary { get; } = "Revoke access or refresh token";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(Handle)
            .AllowAnonymous()
            .Accepts<RevokeRequest>(MediaTypeNames.Application.FormUrlEncoded)
            .Produces(StatusCodes.Status200OK)
            .Produces<RevokeErrorResponse>(StatusCodes.Status400BadRequest);
    }

    private static ProblemHttpResult Handle()
    {
        return TypedResults.Problem(
            title: "OpenIddict revoke endpoint was not handled.",
            detail: "The /connect/revoke route must be intercepted by the OpenIddict server pipeline. " +
                "If this fallback endpoint is executed, OpenIddict revocation endpoint configuration is broken.",
            statusCode: StatusCodes.Status500InternalServerError);
    }
}
