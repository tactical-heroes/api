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
        builder.MapGet(HandleGetAsync)
            .AllowAnonymous()
            .Produces(StatusCodes.Status302Found);

        builder.MapPost(HandlePostAsync)
            .AllowAnonymous()
            .WithName("PostLogout")
            .Accepts<LogoutRequest>(MediaTypeNames.Application.FormUrlEncoded)
            .Produces(StatusCodes.Status302Found);
    }

    private static Task<IResult> HandleGetAsync(HttpContext httpContext)
    {
        return HandleAsync(httpContext);
    }

    private static Task<IResult> HandlePostAsync(HttpContext httpContext)
    {
        return HandleAsync(httpContext);
    }

    private static async Task<IResult> HandleAsync(HttpContext httpContext)
    {
        await httpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

        return TypedResults.SignOut(
            authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
    }
}
