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
        builder.MapGet(HandleGetAsync)
            .AllowAnonymous()
            .AddOpenApiOperationTransformer(AddLogoutQueryParametersAsync)
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

    private static async Task AddLogoutQueryParametersAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        OpenApiSchema stringSchema = await context.GetOrCreateSchemaAsync(
            typeof(string),
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
            Schema = schema
        };
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
