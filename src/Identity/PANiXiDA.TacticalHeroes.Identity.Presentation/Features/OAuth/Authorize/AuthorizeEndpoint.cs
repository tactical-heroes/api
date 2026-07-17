using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using OpenIddict.Server.AspNetCore;

using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.GetDetails;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Common;

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
            .Produces(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> Handle(
        [AsParameters] AuthorizeRequest request,
        HttpContext httpContext,
        IMediator mediator,
        IOptions<OAuthSpaOptions> spaOptions,
        IOptions<OAuthTokenOptions> tokenOptions)
    {
        var openIddictRequest = httpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("OpenIddict server request was not found.");

        var authenticationResult = await httpContext.AuthenticateAsync(
            IdentityConstants.ApplicationScheme);

        if (!authenticationResult.Succeeded || authenticationResult.Principal is null)
        {
            if (openIddictRequest.HasPromptValue(OpenIddictConstants.PromptValues.None))
            {
                return OAuthErrorResults.LoginRequired("Account is not authenticated.");
            }

            return TypedResults.Redirect(
                OAuthLoginRedirectUrlBuilder.Build(
                    spaOptions.Value.LoginUrl,
                    BuildReturnUrl(httpContext, request)));
        }

        var accountIdResult = authenticationResult.Principal.GetSubjectId();
        if (accountIdResult.IsFailure)
        {
            return OAuthErrorResults.LoginRequired("Account is not authenticated.");
        }

        var accountResult = await mediator.QueryAsync(
            new GetAccountDetailsQuery(accountIdResult.Value),
            httpContext.RequestAborted);

        if (accountResult.IsFailure ||
            accountResult.Value.IsBlocked ||
            !accountResult.Value.IsConfirmed)
        {
            return TypedResults.Forbid();
        }

        var principal = OAuthAuthorizationPrincipalFactory.Create(
            authenticationResult.Principal,
            openIddictRequest.GetScopes(),
            tokenOptions.Value.Audience);

        return TypedResults.SignIn(
            principal,
            authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static string BuildReturnUrl(
        HttpContext httpContext,
        AuthorizeRequest request)
    {
        var httpRequest = httpContext.Request;

        return UriHelper.BuildAbsolute(
            httpRequest.Scheme,
            httpRequest.Host,
            httpRequest.PathBase,
            httpRequest.Path,
            QueryString.Create(
                new Dictionary<string, string?>
                {
                    ["client_id"] = request.ClientId,
                    ["request_uri"] = request.RequestUri
                }));
    }
}
