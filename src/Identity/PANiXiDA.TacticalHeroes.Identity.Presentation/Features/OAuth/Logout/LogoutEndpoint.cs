using System.Net.Mime;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OpenApi;

using Microsoft.OpenApi;

using OpenIddict.Server.AspNetCore;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Logout;

internal sealed class LogoutEndpoint : IEndpoint<OAuthEndpoints>
{
    public string Route { get; } = OAuthEndpointRoutes.EndSession;
    public string Name { get; } = "Logout";
    public string Summary { get; } = "Log out user from OpenID Connect";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(builder.Route, handler: HandleGetAsync)
            .AllowAnonymous()
            .AddOpenApiOperationTransformer(AddLogoutQueryParametersAsync)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status302Found);

        builder.MapPost(builder.Route, handler: HandlePostAsync)
            .AllowAnonymous()
            .WithName("PostLogout")
            .Accepts<LogoutRequest>(MediaTypeNames.Application.FormUrlEncoded)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status302Found);
    }

    private static Task<IResult> HandleGetAsync(
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        return HandleAsync(httpContext: httpContext, mediator: mediator, cancellationToken: cancellationToken);
    }

    private static async Task AddLogoutQueryParametersAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        OpenApiSchema stringSchema = await context.GetOrCreateSchemaAsync(
            type: typeof(string),
            parameterDescription: null,
            cancellationToken: cancellationToken);

        operation.Parameters =
        [
            CreateQueryParameter(OpenIddictConstants.Parameters.ClientId, stringSchema),
            CreateQueryParameter(OpenIddictConstants.Parameters.IdTokenHint, stringSchema),
            CreateQueryParameter(OpenIddictConstants.Parameters.PostLogoutRedirectUri, stringSchema),
            CreateQueryParameter(OpenIddictConstants.Parameters.State, stringSchema),
            CreateQueryParameter(OpenIddictConstants.Parameters.UiLocales, stringSchema)
        ];
    }

    private static OpenApiParameter CreateQueryParameter(
        string name,
        OpenApiSchema schema)
    {
        return new OpenApiParameter
        {
            Name = name,
            In = ParameterLocation.Query,
            Required = name == OpenIddictConstants.Parameters.IdTokenHint,
            Schema = schema
        };
    }

    private static Task<IResult> HandlePostAsync(
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        return HandleAsync(httpContext: httpContext, mediator: mediator, cancellationToken: cancellationToken);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(httpContext.GetOpenIddictServerRequest()?.IdTokenHint))
        {
            return TypedResults.BadRequest();
        }

        var authentication = await httpContext.AuthenticateAsync(
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        var principal = authentication.Principal;
        var authorizationId = principal?.GetAuthorizationId();
        if (authorizationId is null)
        {
            return TypedResults.BadRequest();
        }

        if (principal is not null && authorizationId is not null)
        {
            var audiences = principal.GetAudiences();
            if (!Guid.TryParse(principal.GetClaim(OpenIddictConstants.Claims.Subject), out var userId)
                || audiences.Length != 1)
            {
                return TypedResults.Unauthorized();
            }

            var cookie = await httpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
            if (cookie.Succeeded
                && (!Guid.TryParse(cookie.Principal?.GetClaim(OpenIddictConstants.Claims.Subject), out var cookieUserId)
                    || cookieUserId != userId))
            {
                return TypedResults.StatusCode(StatusCodes.Status403Forbidden);
            }

            var result = await mediator.SendAsync(
                LogoutMapper.ToCommand(userId: userId, authorizationId: authorizationId, clientId: audiences[0]),
                cancellationToken);
            if (result.IsFailure)
            {
                return result.ToHttpProblem();
            }
        }

        await httpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

        return TypedResults.SignOut(
            authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
    }
}
