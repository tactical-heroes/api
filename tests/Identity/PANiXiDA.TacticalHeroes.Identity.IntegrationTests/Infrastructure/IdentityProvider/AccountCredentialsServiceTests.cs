using System.Security.Claims;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Claims;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.IdentityProvider;

public sealed class AccountCredentialsServiceTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    private const string Password = "StrongPassword1!";

    [Fact(DisplayName = "LoginAsync should load account and all claims in one read query")]
    public async Task LoginAsync_Should_LoadAccountAndAllClaimsInOneReadQuery()
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
                    ClaimValue = "identity.accounts.manage"
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

        await AddAsync(role, user, cancellationToken);
        Fixture.CommandCounter.Reset();

        await using var scope = Fixture.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IAccountCredentialsService>();

        var result = await service.LoginAsync(
            user.Email,
            Password,
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Claims.ShouldContain(
            new Claim("permission", "identity.profile.read"),
            IdentityClaimComparer.Instance);
        result.Value.Claims.ShouldContain(
            new Claim("permission", "identity.accounts.manage"),
            IdentityClaimComparer.Instance);
        Fixture.CommandCounter.Count.ShouldBe(1);
    }

    private async Task AddAsync(
        ApplicationRole role,
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        await using var scope = Fixture.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        (await roleManager.CreateAsync(role)).Succeeded.ShouldBeTrue();
        (await userManager.CreateAsync(user, Password)).Succeeded.ShouldBeTrue();
        (await userManager.AddToRoleAsync(user, role.Name!)).Succeeded.ShouldBeTrue();

        cancellationToken.ThrowIfCancellationRequested();
    }
}
