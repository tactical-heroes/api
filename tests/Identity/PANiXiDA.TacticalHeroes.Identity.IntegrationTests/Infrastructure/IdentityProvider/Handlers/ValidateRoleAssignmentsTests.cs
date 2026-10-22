using System.Security.Claims;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

using OpenIddict.Abstractions;
using OpenIddict.Server;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Claims;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Handlers;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.IdentityProvider.Handlers;

public sealed class ValidateRoleAssignmentsTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Theory(DisplayName = "Token validation should compare role versions when user access token is validated")]
    [InlineData(0, null, false, true)]
    [InlineData(1, null, true, true)]
    [InlineData(1, "0", true, true)]
    [InlineData(1, "1", false, true)]
    [InlineData(1, "0", false, false)]
    public async Task HandleAsync_Should_ValidateVersion_When_UserAccessTokenIsValidated(
        long version, string? tokenVersion, bool rejected, bool hasAuthorization)
    {
        var ct = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            Email = "version@example.test",
            UserName = "version-user",
            RoleAssignmentsVersion = version
        };
        (await manager.CreateAsync(user, "StrongPassword1!")).Succeeded.ShouldBeTrue();
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(OpenIddictConstants.Claims.Subject, user.Id.ToString())], "test"));
        principal.SetTokenType(OpenIddictConstants.TokenTypeIdentifiers.AccessToken);
        if (hasAuthorization)
        {
            principal.SetAuthorizationId(Guid.CreateVersion7().ToString());
        }

        if (tokenVersion is not null)
        {
            principal.SetClaim(IdentityClaimsFactory.RoleAssignmentsVersionClaimType, tokenVersion);
        }

        var context = new OpenIddictServerEvents.ValidateTokenContext(new OpenIddictServerTransaction
        {
            Options = new OpenIddictServerOptions(),
            Logger = NullLogger.Instance
        })
        {
            Principal = principal,
            CancellationToken = ct
        };
        var handler = new ValidateRoleAssignments(scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>());

        await handler.HandleAsync(context);

        context.IsRejected.ShouldBe(rejected);
    }
}
