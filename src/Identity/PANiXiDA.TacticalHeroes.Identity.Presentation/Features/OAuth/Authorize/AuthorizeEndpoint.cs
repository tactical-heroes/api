using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using OpenIddict.Server.AspNetCore;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetDetails;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Authorize;

internal sealed class AuthorizeEndpoint : IEndpoint<OAuthEndpoints>
{
    public string Route { get; } = OAuthEndpointRoutes.Authorization;
    public string Name { get; } = "Authorize";
    public string Summary { get; } = "Start OpenID Connect authorization";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(Handle)
            .AllowAnonymous()
            .Produces(StatusCodes.Status302Found)
            .Produces(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> Handle(
        [AsParameters] AuthorizeRequest request,
        HttpContext httpContext,
        IMediator mediator,
        IOptions<OAuthTokenOptions> tokenOptions)
    {
        var openIddictRequest = httpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException(message: "OpenIddict server request was not found.");

        var authenticationResult = await httpContext.AuthenticateAsync(
            IdentityConstants.ApplicationScheme);

        if (!authenticationResult.Succeeded || authenticationResult.Principal is null)
        {
            if (openIddictRequest.HasPromptValue(OpenIddictConstants.PromptValues.None))
            {
                return OAuthErrorResults.LoginRequired(description: "User is not authenticated.");
            }

            return TypedResults.Redirect(
                url: OAuthLoginRedirectUrlBuilder.Build(
                    redirectUri: openIddictRequest.RedirectUri,
                    returnUrl: BuildReturnUrl(httpContext: httpContext, request: request)));
        }

        var userIdResult = authenticationResult.Principal.GetSubjectId();
        if (userIdResult.IsFailure)
        {
            return OAuthErrorResults.LoginRequired(description: "User is not authenticated.");
        }

        var userResult = await mediator.QueryAsync(
            new GetUserDetailsQuery(Id: userIdResult.Value),
            httpContext.RequestAborted);

        if (userResult.IsFailure ||
            userResult.Value.IsBlocked ||
            !userResult.Value.IsConfirmed)
        {
            return TypedResults.Forbid();
        }

        var principal = OAuthAuthorizationPrincipalFactory.Create(
            source: authenticationResult.Principal,
            scopes: openIddictRequest.GetScopes(),
            audience: tokenOptions.Value.Audience);

        return TypedResults.SignIn(
            principal: principal,
            authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static string BuildReturnUrl(
        HttpContext httpContext,
        AuthorizeRequest request)
    {
        var httpRequest = httpContext.Request;

        return UriHelper.BuildAbsolute(
            scheme: httpRequest.Scheme,
            host: httpRequest.Host,
            pathBase: httpRequest.PathBase,
            path: httpRequest.Path,
            query: QueryString.Create(
                parameters: new Dictionary<string, string?>
                {
                    ["client_id"] = request.ClientId,
                    ["request_uri"] = request.RequestUri
                }));
    }
}
