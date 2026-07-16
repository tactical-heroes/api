using System.Net.Mime;

using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Logout;

internal sealed class LogoutEndpoint : IEndpoint<OAuthEndpoints>
{
    public string Route { get; } = "/logout";
    public string Name { get; } = "Logout";
    public string Summary { get; } = "Log out user from OpenID Connect";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(HandleGet)
            .AllowAnonymous()
            .Produces(StatusCodes.Status302Found)
            .ProducesProblem(StatusCodes.Status501NotImplemented);

        builder.MapPost(HandlePost)
            .AllowAnonymous()
            .WithName("PostLogout")
            .Accepts<LogoutRequest>(MediaTypeNames.Application.FormUrlEncoded)
            .Produces(StatusCodes.Status302Found)
            .ProducesProblem(StatusCodes.Status501NotImplemented);
    }

    private static IResult HandleGet([AsParameters] LogoutRequest request)
    {
        return EndpointStub.NotImplemented(nameof(LogoutEndpoint));
    }

    private static IResult HandlePost()
    {
        return EndpointStub.NotImplemented(nameof(LogoutEndpoint));
    }
}
