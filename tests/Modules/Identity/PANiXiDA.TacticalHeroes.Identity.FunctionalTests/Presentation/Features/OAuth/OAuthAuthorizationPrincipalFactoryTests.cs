using System.Security.Claims;

using OpenIddict.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Common;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;

public sealed class OAuthAuthorizationPrincipalFactoryTests
{
    [Fact(DisplayName = "Create should include permissions in both tokens when principal has permissions")]
    public void Create_Should_IncludePermissionsInBothTokens_When_PrincipalHasPermissions()
    {
        Claim[] claims =
        [
            new("sub", "user"),
            new("permission", "identity.users.read"),
            new("custom", "private")
        ];

        var principal = OAuthAuthorizationPrincipalFactory.Create(claims, ["openid"], "api");

        principal.FindFirst("permission")!.GetDestinations().ShouldBe(
            [OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken]);
        principal.FindFirst("custom")!.GetDestinations().ShouldBe([OpenIddictConstants.Destinations.AccessToken]);
    }
}
