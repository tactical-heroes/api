using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

using OpenIddict.Server.AspNetCore;

using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.GetUserInfo;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.GetUserInfo;

internal sealed class GetUserInfoEndpoint : IEndpoint<OAuthEndpoints>
{
    public string Route { get; } = "/userinfo";
    public string Name { get; } = "GetUserInfo";
    public string Summary { get; } = "Get OpenID Connect user information";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapMethods([HttpMethods.Get, HttpMethods.Post], Handle)
            .RequireAuthorization(
                new AuthorizeAttribute
                {
                    AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
                })
            .Produces<GetUserInfoResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Handle(
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var accountIdResult = user.GetSubjectId();

        if (accountIdResult.IsFailure)
        {
            return OAuthErrorResults.InvalidToken(description: "Token is invalid.");
        }

        var result = await mediator.QueryAsync(
            new GetUserInfoQuery(AccountId: accountIdResult.Value),
            cancellationToken);

        if (result.IsFailure)
        {
            return OAuthErrorResults.InvalidToken(description: "Token is invalid.");
        }

        return TypedResults.Ok(
            value: GetUserInfoMapper.ToResponse(
                account: result.Value,
                scopes: user.GetScopes()));
    }
}
