using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.GetUserInfo;

internal sealed class GetUserInfoEndpoint : IEndpoint<OAuthEndpoints>
{
    public string Route { get; } = "/userinfo";
    public string Name { get; } = "GetUserInfo";
    public string Summary { get; } = "Get OpenID Connect user information";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapMethods([HttpMethods.Get, HttpMethods.Post], Handle)
            .RequireAuthorization()
            .Produces<GetUserInfoResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status501NotImplemented);
    }

    private static IResult Handle()
    {
        return EndpointStub.NotImplemented(nameof(GetUserInfoEndpoint));
    }
}
