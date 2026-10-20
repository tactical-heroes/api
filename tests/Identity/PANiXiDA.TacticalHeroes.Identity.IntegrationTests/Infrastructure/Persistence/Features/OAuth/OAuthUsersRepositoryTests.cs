using System.Security.Claims;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

using OpenIddict.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Claims;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.OAuth;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Persistence.Features.OAuth;

public sealed class OAuthUsersRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    private const string Password = "StrongPassword1!";

    [Fact(
        DisplayName = "CheckAvailabilityByUserIdAsync should query only availability columns when user has authorization graph")]
    public async Task
        CheckAvailabilityByUserIdAsync_Should_QueryOnlyAvailabilityColumns_When_UserHasAuthorizationGraph()
    {
        var role = new ApplicationRole
        {
            Id = Guid.CreateVersion7(),
            Name = "availability-role",
            Claims = [new ApplicationRoleClaim { ClaimType = "permission", ClaimValue = "identity.users.read" }]
        };
        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            Email = "availability@example.test",
            UserName = "availability-user",
            Status = UserStatus.Active.Name,
            EmailConfirmed = true,
            Claims = [new ApplicationUserClaim { ClaimType = "custom", ClaimValue = "private" }]
        };
        await AddAsync(role, user);
        var commands = new List<string>();
        var options = new DbContextOptionsBuilder<IdentityReadDbContext>()
            .UseNpgsql(Fixture.ConnectionString)
            .UseSnakeCaseNamingConvention()
            .LogTo(commands.Add, [RelationalEventId.CommandExecuted])
            .Options;
        await using var dbContext = new IdentityReadDbContext(options);
        var repository = new OAuthUsersRepository(dbContext);

        var result = await repository.CheckAvailabilityByUserIdAsync(user.Id, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        var sql = commands.ShouldHaveSingleItem();
        sql.ShouldContain("email_confirmed");
        sql.ShouldContain("status");
        sql.ShouldNotContain("asp_net_roles");
        sql.ShouldNotContain("asp_net_user_roles");
        sql.ShouldNotContain("asp_net_user_claims");
        sql.ShouldNotContain("asp_net_role_claims");
        sql.ShouldNotContain("password_hash");
    }

    [Theory(DisplayName = "CheckAvailabilityByUserIdAsync should return forbidden when user is unavailable")]
    [InlineData("Blocked", true)]
    [InlineData("Active", false)]
    public async Task CheckAvailabilityByUserIdAsync_Should_ReturnForbidden_When_UserIsUnavailable(
        string status,
        bool isConfirmed)
    {
        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            Email = "unavailable@example.test",
            UserName = "unavailable-user",
            Status = status,
            EmailConfirmed = isConfirmed
        };
        await AddAsync(new ApplicationRole { Id = Guid.CreateVersion7(), Name = "unavailable-role" }, user);
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOAuthUsersRepository>();

        var result = await repository.CheckAvailabilityByUserIdAsync(user.Id, TestContext.Current.CancellationToken);

        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.Forbidden);
    }

    [Fact(DisplayName = "CheckAvailabilityByUserIdAsync should return not found when user does not exist")]
    public async Task CheckAvailabilityByUserIdAsync_Should_ReturnNotFound_When_UserDoesNotExist()
    {
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOAuthUsersRepository>();

        var result = await repository.CheckAvailabilityByUserIdAsync(
            Guid.CreateVersion7(),
            TestContext.Current.CancellationToken);

        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.NotFound);
    }

    [Fact(DisplayName = "GetExchangeTokenByUserIdAsync should load authorization graph in one query when user exists")]
    public async Task GetExchangeTokenByUserIdAsync_Should_LoadAuthorizationGraphInOneQuery_When_UserExists()
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

    [Fact(DisplayName = "Get user info should return identity state from PostgreSQL when user is available")]
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

    [Fact(DisplayName = "Get user info should reject a blocked user when user is blocked")]
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
