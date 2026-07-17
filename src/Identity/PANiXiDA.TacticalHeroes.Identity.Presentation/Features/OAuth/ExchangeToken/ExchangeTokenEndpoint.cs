using System.Net.Mime;
using System.Security.Claims;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;

using OpenIddict.Server.AspNetCore;

using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.ExchangeToken;
using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.GetClientTokenPrincipal;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.ExchangeToken;

internal sealed class ExchangeTokenEndpoint : IEndpoint<OAuthEndpoints>
{
    public string Route { get; } = "/token";
    public string Name { get; } = "ExchangeToken";
    public string Summary { get; } = "Exchange authorization code or refresh token for tokens";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(Handle)
            .AllowAnonymous()
            .Accepts<ExchangeTokenRequest>(MediaTypeNames.Application.FormUrlEncoded)
            .Produces<ExchangeTokenResponse>(StatusCodes.Status200OK)
            .Produces<ExchangeTokenErrorResponse>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(
        HttpContext httpContext,
        IMediator mediator,
        IOptions<OAuthTokenOptions> options,
        CancellationToken cancellationToken)
    {
        var request = httpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("OpenIddict server request was not found.");

        if (request.IsAuthorizationCodeGrantType())
        {
            return await HandleUserGrantAsync(
                httpContext,
                request,
                mediator,
                options.Value.Audience,
                "Authorization code is invalid.",
                cancellationToken);
        }

        if (request.IsRefreshTokenGrantType())
        {
            return await HandleUserGrantAsync(
                httpContext,
                request,
                mediator,
                options.Value.Audience,
                "Refresh token is invalid.",
                cancellationToken);
        }

        if (request.IsClientCredentialsGrantType())
        {
            return await HandleClientCredentialsGrantAsync(
                request,
                mediator,
                options.Value.Audience,
                cancellationToken);
        }

        if (request.IsTokenExchangeGrantType())
        {
            return await HandleTokenExchangeGrantAsync(
                httpContext,
                request,
                mediator,
                options.Value.Audience,
                cancellationToken);
        }

        return OAuthErrorResults.UnsupportedGrantType("Grant type is not supported.");
    }

    private static async Task<IResult> HandleUserGrantAsync(
        HttpContext httpContext,
        OpenIddictRequest request,
        IMediator mediator,
        string audience,
        string invalidGrantDescription,
        CancellationToken cancellationToken)
    {
        var authenticationResult = await httpContext.AuthenticateAsync(
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        var accountIdResult = authenticationResult.Principal.GetSubjectId();

        if (accountIdResult.IsFailure)
        {
            return OAuthErrorResults.InvalidGrant(invalidGrantDescription);
        }

        var principalResult = await mediator.QueryAsync(
            new ExchangeTokenQuery(accountIdResult.Value),
            cancellationToken);

        return principalResult.IsFailure
            ? OAuthErrorResults.InvalidGrant(invalidGrantDescription)
            : SignInTokenPrincipal(
                request,
                authenticationResult.Principal,
                principalResult.Value.Claims,
                audience);
    }

    private static async Task<IResult> HandleClientCredentialsGrantAsync(
        OpenIddictRequest request,
        IMediator mediator,
        string audience,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ClientId))
        {
            return OAuthErrorResults.InvalidGrant("Client is invalid.");
        }

        var principalResult = await mediator.QueryAsync(
            new GetClientTokenPrincipalQuery(request.ClientId),
            cancellationToken);

        return principalResult.IsFailure
            ? OAuthErrorResults.InvalidGrant("Client is invalid.")
            : SignInTokenPrincipal(
                request,
                sourcePrincipal: null,
                principalResult.Value.Claims,
                audience);
    }

    private static async Task<IResult> HandleTokenExchangeGrantAsync(
        HttpContext httpContext,
        OpenIddictRequest request,
        IMediator mediator,
        string audience,
        CancellationToken cancellationToken)
    {
        var authenticationResult = await httpContext.AuthenticateAsync(
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        var subject = authenticationResult.Principal?.GetClaim(
            OpenIddictConstants.Claims.Subject);

        if (string.IsNullOrWhiteSpace(subject))
        {
            return OAuthErrorResults.InvalidGrant("Subject token is invalid.");
        }

        if (Guid.TryParse(subject, out var accountId))
        {
            var accountResult = await mediator.QueryAsync(
                new ExchangeTokenQuery(accountId),
                cancellationToken);

            return accountResult.IsFailure
                ? OAuthErrorResults.InvalidGrant("Subject token is invalid.")
                : SignInTokenPrincipal(
                    request,
                    authenticationResult.Principal,
                    accountResult.Value.Claims,
                    audience);
        }

        var clientResult = await mediator.QueryAsync(
            new GetClientTokenPrincipalQuery(subject),
            cancellationToken);

        return clientResult.IsFailure
            ? OAuthErrorResults.InvalidGrant("Subject token is invalid.")
            : SignInTokenPrincipal(
                request,
                authenticationResult.Principal,
                clientResult.Value.Claims,
                audience);
    }

    private static SignInHttpResult SignInTokenPrincipal(
        OpenIddictRequest request,
        ClaimsPrincipal? sourcePrincipal,
        IReadOnlyCollection<Claim> claims,
        string audience)
    {
        var scopes = OAuthRequestScopes.GetRequestedOrPrincipalScopes(
            request,
            sourcePrincipal);
        var principal = OAuthAuthorizationPrincipalFactory.Create(
            claims,
            scopes,
            audience);

        return TypedResults.SignIn(
            principal,
            authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}
