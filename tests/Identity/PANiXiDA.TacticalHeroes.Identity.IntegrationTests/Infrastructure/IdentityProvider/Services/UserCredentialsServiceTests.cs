using System.Security.Claims;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Claims;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.IdentityProvider.Services;

public sealed class UserCredentialsServiceTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    private const string Password = "StrongPassword1!";

    [Fact(DisplayName = "LoginAsync should load user and all claims in one read query")]
    public async Task LoginAsync_Should_LoadUserAndAllClaimsInOneReadQuery()
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
            Email = "login@example.com",
            UserName = "login-user",
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
        var service = scope.ServiceProvider.GetRequiredService<IUserCredentialsService>();

        var result = await service.LoginAsync(
            user.Email,
            Password,
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Claims.ShouldContain(
            new Claim(type: "permission", value: "identity.profile.read"),
            IdentityClaimComparer.Instance);
        result.Value.Claims.ShouldContain(
            new Claim(type: "permission", value: "identity.users.manage"),
            IdentityClaimComparer.Instance);
        Fixture.CommandCounter.Count.ShouldBe(1);
    }

    [Fact(DisplayName = "Login should return unauthorized for an invalid password")]
    public async Task LoginAsync_Should_ReturnUnauthorized_When_PasswordIsInvalid()
    {
        var role = new ApplicationRole
        {
            Id = Guid.CreateVersion7(),
            Name = "login-role"
        };
        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            Email = "invalid-password@example.com",
            UserName = "invalid-password-hero",
            EmailConfirmed = true,
            Status = UserStatus.Active.Name,
            LockoutEnabled = true
        };
        await AddAsync(role, user);

        await using var scope = Fixture.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IUserCredentialsService>();

        var result = await service.LoginAsync(
            user.Email,
            "WrongPassword1!",
            TestContext.Current.CancellationToken);

        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.Unauthorized);
    }

    [Fact(DisplayName = "Change password should replace credentials stored by ASP.NET Core Identity")]
    public async Task ChangePasswordAsync_Should_ReplacePassword_When_CurrentPasswordIsValid()
    {
        var role = new ApplicationRole
        {
            Id = Guid.CreateVersion7(),
            Name = "password-role"
        };
        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            Email = "change-password@example.com",
            UserName = "change-password-hero",
            EmailConfirmed = true,
            Status = UserStatus.Active.Name,
            LockoutEnabled = true
        };
        await AddAsync(role, user);

        await using var scope = Fixture.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IUserCredentialsService>();
        var cancellationToken = TestContext.Current.CancellationToken;

        var changeResult = await service.ChangePasswordAsync(
            user.Id,
            Password,
            "NewStrongPassword1!",
            cancellationToken);
        var loginResult = await service.LoginAsync(
            user.Email,
            "NewStrongPassword1!",
            cancellationToken);

        changeResult.IsSuccess.ShouldBeTrue();
        loginResult.IsSuccess.ShouldBeTrue();
        loginResult.Value.Id.ShouldBe(user.Id);
    }

    private async Task AddAsync(
        ApplicationRole role,
        ApplicationUser user)
    {
        await using var scope = Fixture.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        (await roleManager.CreateAsync(role: role)).Succeeded.ShouldBeTrue();
        (await userManager.CreateAsync(user: user, password: Password)).Succeeded.ShouldBeTrue();
        (await userManager.AddToRoleAsync(user, role.Name!)).Succeeded.ShouldBeTrue();
    }
}
