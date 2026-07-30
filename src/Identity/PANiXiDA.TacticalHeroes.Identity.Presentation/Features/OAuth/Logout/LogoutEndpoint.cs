using System.Net.Mime;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

using OpenIddict.Server.AspNetCore;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Logout;

internal sealed class LogoutEndpoint : IEndpoint<OAuthEndpoints>
{
    public string Route { get; } = OAuthEndpointRoutes.EndSession;
    public string Name { get; } = "Logout";
    public string Summary { get; } = "Log out user from OpenID Connect";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(handler: HandleGet)
            .AllowAnonymous()
            .Produces(statusCode: StatusCodes.Status302Found);

        builder.MapPost(handler: HandlePost)
            .AllowAnonymous()
            .WithName(endpointName: "PostLogout")
            .Accepts<LogoutRequest>(contentType: MediaTypeNames.Application.FormUrlEncoded)
            .Produces(statusCode: StatusCodes.Status302Found);
    }

    private static Task<IResult> HandleGet(
        [AsParameters] LogoutRequest request,
        HttpContext httpContext)
    {
        return Handle(httpContext: httpContext);
    }

    private static Task<IResult> HandlePost(HttpContext httpContext)
    {
        return Handle(httpContext: httpContext);
    }

    private static async Task<IResult> Handle(HttpContext httpContext)
    {
        await httpContext.SignOutAsync(scheme: IdentityConstants.ApplicationScheme);

        return TypedResults.SignOut(
            authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
    }
}
