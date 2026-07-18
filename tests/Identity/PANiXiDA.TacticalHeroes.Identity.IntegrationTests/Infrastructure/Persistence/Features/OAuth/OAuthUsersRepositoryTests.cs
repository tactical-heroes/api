using System.Security.Claims;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

using OpenIddict.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Claims;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Persistence.Features.OAuth;

public sealed class OAuthUsersRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    private const string Password = "StrongPassword1!";

    [Fact(DisplayName = "GetExchangeTokenByUserIdAsync should load authorization graph in one query")]
    public async Task GetExchangeTokenByUserIdAsync_Should_LoadAuthorizationGraphInOneQuery()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var role = new ApplicationRole
        {
            Id = Guid.CreateVersion7(),
            Name = "admin",
            Claims =
            [
                new ApplicationRoleClaim
                {
                    ClaimType = "permission",
                    ClaimValue = "identity.users.manage"
                }
            ]
        };
        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            Email = "hero@example.com",
            UserName = "hero",
            EmailConfirmed = true,
            Status = UserStatus.Active.Name,
            LockoutEnabled = true,
            Claims =
            [
                new ApplicationUserClaim
                {
                    ClaimType = "permission",
                    ClaimValue = "identity.profile.read"
                }
            ]
        };

        await AddAsync(role, user);
        Fixture.CommandCounter.Reset();

        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOAuthUsersRepository>();

        var result = await repository.GetExchangeTokenByUserIdAsync(
            user.Id,
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Claims.ShouldContain(
            new Claim(type: "permission", value: "identity.profile.read"),
            IdentityClaimComparer.Instance);
        result.Value.Claims.ShouldContain(
            new Claim(type: "permission", value: "identity.users.manage"),
            IdentityClaimComparer.Instance);
        result.Value.Claims.ShouldContain(
            new Claim(type: OpenIddictConstants.Claims.Role, value: "admin"),
            IdentityClaimComparer.Instance);
        Fixture.CommandCounter.Count.ShouldBe(1);
    }

    [Fact(DisplayName = "Get user info should return identity state from PostgreSQL")]
    public async Task GetUserInfoByUserIdAsync_Should_ReturnIdentityState_When_UserIsAvailable()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var role = new ApplicationRole
        {
            Id = Guid.CreateVersion7(),
            Name = "operator"
        };
        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            Email = "userinfo@example.com",
            UserName = "userinfo-hero",
            EmailConfirmed = true,
            Status = UserStatus.Active.Name,
            LockoutEnabled = true
        };

        await AddAsync(role, user);

        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOAuthUsersRepository>();

        var result = await repository.GetUserInfoByUserIdAsync(user.Id, cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.UserId.ShouldBe(user.Id);
        result.Value.Name.ShouldBe("userinfo-hero");
        result.Value.Email.ShouldBe("userinfo@example.com");
        result.Value.EmailVerified.ShouldBe(true);
        result.Value.Roles.ShouldContain("operator");
    }

    [Fact(DisplayName = "Get user info should reject a blocked user")]
    public async Task GetUserInfoByUserIdAsync_Should_ReturnForbidden_When_UserIsBlocked()
    {
        var role = new ApplicationRole
        {
            Id = Guid.CreateVersion7(),
            Name = "blocked-role"
        };
        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            Email = "blocked@example.com",
            UserName = "blocked-hero",
            EmailConfirmed = true,
            Status = UserStatus.Blocked.Name,
            LockoutEnabled = true
        };

        await AddAsync(role, user);

        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOAuthUsersRepository>();

        var result = await repository.GetUserInfoByUserIdAsync(
            user.Id,
            TestContext.Current.CancellationToken);

        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.Forbidden);
    }

    private async Task AddAsync(
        ApplicationRole role,
        ApplicationUser user)
    {
        await using var scope = Fixture.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var createRoleResult = await roleManager.CreateAsync(role: role);
        createRoleResult.Succeeded.ShouldBeTrue();

        var createUserResult = await userManager.CreateAsync(user: user, password: Password);
        createUserResult.Succeeded.ShouldBeTrue();

        var assignRoleResult = await userManager.AddToRoleAsync(user, role.Name!);
        assignRoleResult.Succeeded.ShouldBeTrue();
    }
}
