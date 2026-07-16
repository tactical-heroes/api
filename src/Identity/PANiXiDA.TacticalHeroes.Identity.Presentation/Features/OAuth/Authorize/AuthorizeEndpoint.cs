using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Authorize;

internal sealed class AuthorizeEndpoint : IEndpoint<OAuthEndpoints>
{
    public string Route { get; } = "/authorize";
    public string Name { get; } = "Authorize";
    public string Summary { get; } = "Start OpenID Connect authorization";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(Handle)
            .AllowAnonymous()
            .Produces(StatusCodes.Status302Found)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status501NotImplemented);
    }

    private static IResult Handle([AsParameters] AuthorizeRequest request)
    {
        return EndpointStub.NotImplemented(nameof(AuthorizeEndpoint));
    }
}
