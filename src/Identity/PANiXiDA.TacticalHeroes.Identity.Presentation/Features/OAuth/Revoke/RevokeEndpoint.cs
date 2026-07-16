using System.Net.Mime;

using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

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
            .Produces<RevokeErrorResponse>(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status501NotImplemented);
    }

    private static IResult Handle()
    {
        return EndpointStub.NotImplemented(nameof(RevokeEndpoint));
    }
}
