using System.Net.Mime;

using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

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
            .Produces<ExchangeTokenErrorResponse>(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status501NotImplemented);
    }

    private static IResult Handle()
    {
        return EndpointStub.NotImplemented(nameof(ExchangeTokenEndpoint));
    }
}
