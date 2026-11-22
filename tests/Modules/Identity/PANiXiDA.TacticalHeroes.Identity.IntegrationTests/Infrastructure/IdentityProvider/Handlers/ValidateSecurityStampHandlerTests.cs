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

public sealed class ValidateSecurityStampHandlerTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
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
            var authorizations = scope.ServiceProvider.GetRequiredService<IOpenIddictAuthorizationManager>();
            var authorization = await authorizations.CreateAsync(
                new OpenIddictAuthorizationDescriptor
                {
                    Subject = user.Id.ToString(),
                    Type = OpenIddictConstants.AuthorizationTypes.AdHoc,
                    Status = OpenIddictConstants.Statuses.Valid
                },
                ct);
            principal.SetAuthorizationId(await authorizations.GetIdAsync(authorization, ct));
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
        var handler = new ValidateSecurityStampHandler(
            scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>(), options);

        await handler.HandleAsync(context);

        context.IsRejected.ShouldBe(rejected);
    }

    [Theory(DisplayName = "Token validation should require a valid grant when user token is validated")]
    [InlineData(OpenIddictConstants.TokenTypeIdentifiers.AccessToken, "valid", false)]
    [InlineData(OpenIddictConstants.TokenTypeIdentifiers.AccessToken, "revoked", true)]
    [InlineData(OpenIddictConstants.TokenTypeIdentifiers.AccessToken, "missing", true)]
    [InlineData(OpenIddictConstants.TokenTypeIdentifiers.RefreshToken, "valid", false)]
    [InlineData(OpenIddictConstants.TokenTypeIdentifiers.RefreshToken, "revoked", true)]
    [InlineData(OpenIddictConstants.TokenTypeIdentifiers.RefreshToken, "foreign", true)]
    [InlineData(OpenIddictConstants.TokenTypeIdentifiers.Private.AuthorizationCode, "valid", false)]
    [InlineData(OpenIddictConstants.TokenTypeIdentifiers.Private.AuthorizationCode, "revoked", true)]
    [InlineData(OpenIddictConstants.TokenTypeIdentifiers.Private.AuthorizationCode, "malformed", true)]
    public async Task HandleAsync_Should_ValidateGrant_When_UserTokenIsValidated(
        string tokenType, string mode, bool rejected)
    {
        var ct = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            UserName = "grant-user",
            Email = "grant@example.test"
        };
        (await manager.CreateAsync(user, "StrongPassword1!")).Succeeded.ShouldBeTrue();
        var authorizations = scope.ServiceProvider.GetRequiredService<IOpenIddictAuthorizationManager>();
        var authorization = await authorizations.CreateAsync(
            new OpenIddictAuthorizationDescriptor
            {
                Subject = mode == "foreign" ? Guid.CreateVersion7().ToString() : user.Id.ToString(),
                Type = OpenIddictConstants.AuthorizationTypes.AdHoc,
                Status = mode == "revoked" ? OpenIddictConstants.Statuses.Revoked : OpenIddictConstants.Statuses.Valid
            },
            ct);
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(OpenIddictConstants.Claims.Subject, user.Id.ToString())], "test"));
        principal.SetTokenType(tokenType);
        principal.SetAuthorizationId(mode switch
        {
            "missing" => Guid.CreateVersion7().ToString(),
            "malformed" => "invalid",
            _ => await authorizations.GetIdAsync(authorization, ct)
        });
        var options = scope.ServiceProvider.GetRequiredService<IOptions<IdentityOptions>>();
        principal.SetClaim(
            options.Value.ClaimsIdentity.SecurityStampClaimType,
            tokenType == OpenIddictConstants.TokenTypeIdentifiers.AccessToken ? user.SecurityStamp : "old-stamp");
        var context = new OpenIddictServerEvents.ValidateTokenContext(new OpenIddictServerTransaction
        {
            Options = new OpenIddictServerOptions(),
            Logger = NullLogger.Instance
        })
        {
            Principal = principal,
            CancellationToken = ct
        };
        var handler = new ValidateSecurityStampHandler(
            scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>(), options);

        await handler.HandleAsync(context);

        context.IsRejected.ShouldBe(rejected);
    }
}
