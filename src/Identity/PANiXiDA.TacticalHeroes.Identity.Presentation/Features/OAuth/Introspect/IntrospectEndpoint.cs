using System.Net.Mime;

using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Introspect;

internal sealed class IntrospectEndpoint : IEndpoint<OAuthEndpoints>
{
    public string Route { get; } = "/introspect";
    public string Name { get; } = "IntrospectToken";
    public string Summary { get; } = "Introspect access or refresh token";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(Handle)
            .AllowAnonymous()
            .Accepts<IntrospectRequest>(MediaTypeNames.Application.FormUrlEncoded)
            .Produces<IntrospectResponse>(StatusCodes.Status200OK)
            .Produces<IntrospectErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<IntrospectErrorResponse>(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status501NotImplemented);
    }

    private static IResult Handle()
    {
        return EndpointStub.NotImplemented(nameof(IntrospectEndpoint));
    }
}
