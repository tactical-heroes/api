using System.Net.Mime;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

using OpenIddict.Server.AspNetCore;

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
            .Produces(StatusCodes.Status302Found);

        builder.MapPost((Func<HttpContext, Task<IResult>>)HandlePost)
            .AllowAnonymous()
            .WithName("PostLogout")
            .Accepts<LogoutRequest>(MediaTypeNames.Application.FormUrlEncoded)
            .Produces(StatusCodes.Status302Found);
    }

    private static Task<IResult> HandleGet(
        [AsParameters] LogoutRequest request,
        HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(request);

        return Handle(httpContext);
    }

    private static Task<IResult> HandlePost(HttpContext httpContext)
    {
        return Handle(httpContext);
    }

    private static async Task<IResult> Handle(HttpContext httpContext)
    {
        await httpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

        return TypedResults.SignOut(
            authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
    }
}
