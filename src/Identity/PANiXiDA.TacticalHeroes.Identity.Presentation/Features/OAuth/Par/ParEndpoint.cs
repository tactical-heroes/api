using System.Net.Mime;

using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Par;

internal sealed class ParEndpoint : IEndpoint<OAuthEndpoints>
{
    public string Route { get; } = "/par";
    public string Name { get; } = "PushAuthorizationRequest";
    public string Summary { get; } = "Create pushed authorization request";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(Handle)
            .AllowAnonymous()
            .Accepts<ParRequest>(MediaTypeNames.Application.FormUrlEncoded)
            .Produces<ParResponse>(StatusCodes.Status201Created)
            .Produces<ParErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ParErrorResponse>(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status501NotImplemented);
    }

    private static IResult Handle()
    {
        return EndpointStub.NotImplemented(nameof(ParEndpoint));
    }
}
