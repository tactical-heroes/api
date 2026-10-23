using System.Security.Claims;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using OpenIddict.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Claims;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Auth;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Persistence.Features.Auth;

public sealed class AuthenticatedUserReaderTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "Authenticated user reader should load current identity in one query when user exists")]
    public async Task GetByIdAsync_Should_LoadCurrentIdentityInOneQuery_When_UserExists()
    {
        var ct = TestContext.Current.CancellationToken;
        var userId = await AddUserAsync(UserStatus.Active.Name, true);
        await using var scope = Fixture.CreateScope();
        var options = new IdentityOptions();
        options.ClaimsIdentity.SecurityStampClaimType = "custom_stamp";
        var reader = new AuthenticatedUserReader(
            scope.ServiceProvider.GetRequiredService<IdentityReadDbContext>(), Options.Create(options));
        Fixture.CommandCounter.Reset();

        var result = await reader.GetByIdAsync(userId, ct);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(userId);
        result.Value.Email.ShouldBe("auth-reader@example.test");
        result.Value.UserName.ShouldBe("auth-reader");
        var claims = result.Value.Claims;
        claims.ShouldContain(new Claim("permission", "identity.users.read"), IdentityClaimComparer.Instance);
        claims.ShouldContain(new Claim("permission", "identity.roles.read"), IdentityClaimComparer.Instance);
        claims.ShouldContain(new Claim(OpenIddictConstants.Claims.Role, "auth-role"), IdentityClaimComparer.Instance);
        claims.ShouldContain(claim => claim.Type == "custom_stamp" && !string.IsNullOrEmpty(claim.Value));
        claims.ShouldNotContain(claim => claim.Type == new IdentityOptions().ClaimsIdentity.SecurityStampClaimType);
        Fixture.CommandCounter.Count.ShouldBe(1);
    }

    [Theory(DisplayName = "Authenticated user reader should reject unavailable accounts when user is unavailable")]
    [InlineData("Blocked", true)]
    [InlineData("Active", false)]
    public async Task GetByIdAsync_Should_RejectUnavailableAccounts_When_UserIsUnavailable(
        string status, bool confirmed)
    {
        var userId = await AddUserAsync(status, confirmed);
        await using var scope = Fixture.CreateScope();
        var reader = scope.ServiceProvider.GetRequiredService<IAuthenticatedUserReader>();

        var result = await reader.GetByIdAsync(userId, TestContext.Current.CancellationToken);

        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.Forbidden);
    }

    [Fact(DisplayName = "Authenticated user reader should return not found when user does not exist")]
    public async Task GetByIdAsync_Should_ReturnNotFound_When_UserDoesNotExist()
    {
        await using var scope = Fixture.CreateScope();
        var reader = scope.ServiceProvider.GetRequiredService<IAuthenticatedUserReader>();

        var result = await reader.GetByIdAsync(Guid.CreateVersion7(), TestContext.Current.CancellationToken);

        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.NotFound);
    }

    [Fact(DisplayName = "Authenticated user reader should return fresh claims when user changes between reads")]
    public async Task GetByIdAsync_Should_ReturnFreshClaims_When_UserChangesBetweenReads()
    {
        var userId = await AddUserAsync(UserStatus.Active.Name, true);
        var ct = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var reader = scope.ServiceProvider.GetRequiredService<IAuthenticatedUserReader>();
        var before = await reader.GetByIdAsync(userId, ct);
        await using (var writer = Fixture.CreateScope())
        {
            var manager = writer.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await manager.FindByIdAsync(userId.ToString());
            user.ShouldNotBeNull();
            user.UserName = "updated-auth-reader";
            (await manager.UpdateAsync(user)).Succeeded.ShouldBeTrue();
            (await manager.UpdateSecurityStampAsync(user)).Succeeded.ShouldBeTrue();
        }

        var after = await reader.GetByIdAsync(userId, ct);

        after.Value.UserName.ShouldBe("updated-auth-reader");
        var stampType = new IdentityOptions().ClaimsIdentity.SecurityStampClaimType;
        after.Value.Claims.Single(claim => claim.Type == stampType).Value
            .ShouldNotBe(before.Value.Claims.Single(claim => claim.Type == stampType).Value);
    }

    private async Task<Guid> AddUserAsync(string status, bool confirmed)
    {
        await using var scope = Fixture.CreateScope();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var role = new ApplicationRole
        {
            Id = Guid.CreateVersion7(),
            Name = "auth-role",
            Claims = [new ApplicationRoleClaim { ClaimType = "permission", ClaimValue = "identity.roles.read" }]
        };
        (await roles.CreateAsync(role)).Succeeded.ShouldBeTrue();
        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            Email = "auth-reader@example.test",
            UserName = "auth-reader",
            EmailConfirmed = confirmed,
            Status = status,
            Claims = [new ApplicationUserClaim { ClaimType = "permission", ClaimValue = "identity.users.read" }]
        };
        (await users.CreateAsync(user, "StrongPassword1!")).Succeeded.ShouldBeTrue();
        (await users.AddToRoleAsync(user, role.Name)).Succeeded.ShouldBeTrue();
        return user.Id;
    }
}
