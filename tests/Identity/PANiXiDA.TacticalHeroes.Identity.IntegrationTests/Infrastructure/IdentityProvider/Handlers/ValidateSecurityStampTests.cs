using System.Security.Claims;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using OpenIddict.Abstractions;
using OpenIddict.Server;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Handlers;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.IdentityProvider.Handlers;

public sealed class ValidateSecurityStampTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Theory(DisplayName = "Token validation should compare security stamps when user access token is validated")]
    [InlineData(null, true, true, false)]
    [InlineData("old-stamp", true, true, false)]
    [InlineData("current", false, true, false)]
    [InlineData("old-stamp", false, false, false)]
    [InlineData("current", true, true, true)]
    public async Task HandleAsync_Should_ValidateStamp_When_UserAccessTokenIsValidated(
        string? tokenStamp, bool rejected, bool hasAuthorization, bool userDeleted)
    {
        var ct = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            Email = "stamp@example.test",
            UserName = "stamp-user"
        };
        (await manager.CreateAsync(user, "StrongPassword1!")).Succeeded.ShouldBeTrue();
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(OpenIddictConstants.Claims.Subject, user.Id.ToString())], "test"));
        principal.SetTokenType(OpenIddictConstants.TokenTypeIdentifiers.AccessToken);
        if (hasAuthorization)
        {
            principal.SetAuthorizationId(Guid.CreateVersion7().ToString());
        }

        var options = scope.ServiceProvider.GetRequiredService<IOptions<IdentityOptions>>();
        if (tokenStamp is not null)
        {
            principal.SetClaim(
                options.Value.ClaimsIdentity.SecurityStampClaimType,
                tokenStamp == "current" ? user.SecurityStamp : tokenStamp);
        }

        if (userDeleted)
        {
            (await manager.DeleteAsync(user)).Succeeded.ShouldBeTrue();
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
        var handler = new ValidateSecurityStamp(
            scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>(), options);

        await handler.HandleAsync(context);

        context.IsRejected.ShouldBe(rejected);
    }
}
