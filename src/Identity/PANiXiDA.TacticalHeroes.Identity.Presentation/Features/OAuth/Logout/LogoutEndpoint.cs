using System.Net.Mime;

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
        builder.MapGet(pattern: builder.Route, handler: HandleGetAsync)
            .AllowAnonymous()
            .AddOpenApiOperationTransformer(transformer: AddLogoutQueryParametersAsync)
            .Produces(StatusCodes.Status302Found);

        builder.MapPost(pattern: builder.Route, handler: HandlePostAsync)
            .AllowAnonymous()
            .WithName(endpointName: "PostLogout")
            .Accepts<LogoutRequest>(MediaTypeNames.Application.FormUrlEncoded)
            .Produces(StatusCodes.Status302Found);
    }

    private static Task<IResult> HandleGetAsync(
        HttpContext httpContext)
    {
        return HandleAsync(httpContext: httpContext);
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
            CreateQueryParameter(name: OpenIddictConstants.Parameters.ClientId, schema: stringSchema),
            CreateQueryParameter(name: OpenIddictConstants.Parameters.IdTokenHint, schema: stringSchema),
            CreateQueryParameter(name: OpenIddictConstants.Parameters.PostLogoutRedirectUri, schema: stringSchema),
            CreateQueryParameter(name: OpenIddictConstants.Parameters.State, schema: stringSchema),
            CreateQueryParameter(name: OpenIddictConstants.Parameters.UiLocales, schema: stringSchema)
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
            Schema = schema
        };
    }

    private static Task<IResult> HandlePostAsync(
        HttpContext httpContext)
    {
        return HandleAsync(httpContext: httpContext);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext)
    {
        await httpContext.SignOutAsync(scheme: IdentityConstants.ApplicationScheme);

        return TypedResults.SignOut(
            authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
    }
}
