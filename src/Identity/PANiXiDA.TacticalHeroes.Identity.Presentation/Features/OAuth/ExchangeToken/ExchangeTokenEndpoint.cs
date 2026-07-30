using System.Net.Mime;
using System.Security.Claims;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;

using OpenIddict.Server.AspNetCore;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.ExchangeToken;

internal sealed class ExchangeTokenEndpoint : IEndpoint<OAuthEndpoints>
{
    public string Route { get; } = OAuthEndpointRoutes.Token;
    public string Name { get; } = "ExchangeToken";
    public string Summary { get; } = "Exchange authorization code or refresh token for tokens";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(handler: Handle)
            .AllowAnonymous()
            .Accepts<ExchangeTokenRequest>(contentType: MediaTypeNames.Application.FormUrlEncoded)
            .Produces<ExchangeTokenResponse>(statusCode: StatusCodes.Status200OK)
            .Produces<ExchangeTokenErrorResponse>(statusCode: StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(
        HttpContext httpContext,
        IMediator mediator,
        IOptions<OAuthTokenOptions> options,
        CancellationToken cancellationToken)
    {
        var request = httpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException(message: "OpenIddict server request was not found.");

        if (request.IsAuthorizationCodeGrantType())
        {
            return await HandleUserGrantAsync(
                httpContext: httpContext,
                request: request,
                mediator: mediator,
                audience: options.Value.Audience,
                invalidGrantDescription: "Authorization code is invalid.",
                cancellationToken: cancellationToken);
        }

        if (request.IsRefreshTokenGrantType())
        {
            return await HandleUserGrantAsync(
                httpContext: httpContext,
                request: request,
                mediator: mediator,
                audience: options.Value.Audience,
                invalidGrantDescription: "Refresh token is invalid.",
                cancellationToken: cancellationToken);
        }

        if (request.IsClientCredentialsGrantType())
        {
            return await HandleClientCredentialsGrantAsync(
                request: request,
                mediator: mediator,
                audience: options.Value.Audience,
                cancellationToken: cancellationToken);
        }

        if (request.IsTokenExchangeGrantType())
        {
            return await HandleTokenExchangeGrantAsync(
                httpContext: httpContext,
                request: request,
                mediator: mediator,
                audience: options.Value.Audience,
                cancellationToken: cancellationToken);
        }

        return OAuthErrorResults.UnsupportedGrantType(description: "Grant type is not supported.");
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
            scheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        var userIdResult = authenticationResult.Principal.GetSubjectId();

        if (userIdResult.IsFailure)
        {
            return OAuthErrorResults.InvalidGrant(description: invalidGrantDescription);
        }

        var principalResult = await mediator.QueryAsync(
            query: ExchangeTokenMapper.ToUserQuery(userId: userIdResult.Value),
            cancellationToken: cancellationToken);

        return principalResult.IsFailure
            ? OAuthErrorResults.InvalidGrant(description: invalidGrantDescription)
            : SignInTokenPrincipal(
                request: request,
                sourcePrincipal: authenticationResult.Principal,
                claims: principalResult.Value.Claims,
                audience: audience);
    }

    private static async Task<IResult> HandleClientCredentialsGrantAsync(
        OpenIddictRequest request,
        IMediator mediator,
        string audience,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(value: request.ClientId))
        {
            return OAuthErrorResults.InvalidGrant(description: "Client is invalid.");
        }

        var principalResult = await mediator.QueryAsync(
            query: ExchangeTokenMapper.ToClientQuery(clientId: request.ClientId),
            cancellationToken: cancellationToken);

        return principalResult.IsFailure
            ? OAuthErrorResults.InvalidGrant(description: "Client is invalid.")
            : SignInTokenPrincipal(
                request: request,
                sourcePrincipal: null,
                claims: principalResult.Value.Claims,
                audience: audience);
    }

    private static async Task<IResult> HandleTokenExchangeGrantAsync(
        HttpContext httpContext,
        OpenIddictRequest request,
        IMediator mediator,
        string audience,
        CancellationToken cancellationToken)
    {
        var authenticationResult = await httpContext.AuthenticateAsync(
            scheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        var subject = authenticationResult.Principal?.GetClaim(
            type: OpenIddictConstants.Claims.Subject);

        if (string.IsNullOrWhiteSpace(value: subject))
        {
            return OAuthErrorResults.InvalidGrant(description: "Subject token is invalid.");
        }

        if (Guid.TryParse(input: subject, result: out var userId))
        {
            var userResult = await mediator.QueryAsync(
                query: ExchangeTokenMapper.ToUserQuery(userId: userId),
                cancellationToken: cancellationToken);

            return userResult.IsFailure
                ? OAuthErrorResults.InvalidGrant(description: "Subject token is invalid.")
                : SignInTokenPrincipal(
                    request: request,
                    sourcePrincipal: authenticationResult.Principal,
                    claims: userResult.Value.Claims,
                    audience: audience);
        }

        var clientResult = await mediator.QueryAsync(
            query: ExchangeTokenMapper.ToClientQuery(clientId: subject),
            cancellationToken: cancellationToken);

        return clientResult.IsFailure
            ? OAuthErrorResults.InvalidGrant(description: "Subject token is invalid.")
            : SignInTokenPrincipal(
                request: request,
                sourcePrincipal: authenticationResult.Principal,
                claims: clientResult.Value.Claims,
                audience: audience);
    }

    private static SignInHttpResult SignInTokenPrincipal(
        OpenIddictRequest request,
        ClaimsPrincipal? sourcePrincipal,
        IReadOnlyCollection<Claim> claims,
        string audience)
    {
        var scopes = OAuthRequestScopes.GetRequestedOrPrincipalScopes(
            request: request,
            principal: sourcePrincipal);
        var principal = OAuthAuthorizationPrincipalFactory.Create(
            claims: claims,
            scopes: scopes,
            audience: audience);

        return TypedResults.SignIn(
            principal: principal,
            authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}
