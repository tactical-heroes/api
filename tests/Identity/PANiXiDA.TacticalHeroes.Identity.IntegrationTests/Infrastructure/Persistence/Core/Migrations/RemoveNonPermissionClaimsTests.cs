using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core.Migrations;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Persistence.Core.Migrations;

public sealed class RemoveNonPermissionClaimsTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "Up should retain only permission claims when assignments exist")]
    public async Task Up_Should_RetainOnlyPermissionClaims_When_AssignmentsExist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await Fixture.SeedRolesAsync(cancellationToken);
        await using var scope = Fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var role = await context.Set<ApplicationRole>().Include(item => item.Claims)
            .SingleAsync(item => item.Name == "Developer", cancellationToken);
        role.Claims.Add(new ApplicationRoleClaim { ClaimType = " SECTION ", ClaimValue = "custom-module" });
        role.Claims.Add(new ApplicationRoleClaim { ClaimType = "custom", ClaimValue = "section" });
        role.Claims.Add(new ApplicationRoleClaim { ClaimType = string.Empty, ClaimValue = "permission" });
        role.Claims.Add(new ApplicationRoleClaim { ClaimType = " ", ClaimValue = "permission" });
        role.Claims.Add(new ApplicationRoleClaim { ClaimType = " PERMISSION ", ClaimValue = "retired.permission" });
        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            UserName = "migration-test",
            Email = "migration-test@example.com",
            Claims =
            [
                new ApplicationUserClaim { ClaimType = "section", ClaimValue = "identity" },
                new ApplicationUserClaim { ClaimType = " SECTION ", ClaimValue = "compendium" },
                new ApplicationUserClaim { ClaimType = "permission", ClaimValue = "identity.users.read" },
                new ApplicationUserClaim { ClaimType = "custom", ClaimValue = "section" },
                new ApplicationUserClaim { ClaimType = string.Empty, ClaimValue = "permission" },
                new ApplicationUserClaim { ClaimType = " ", ClaimValue = "permission" },
                new ApplicationUserClaim { ClaimType = " PERMISSION ", ClaimValue = "retired.permission" }
            ]
        };
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var created = await userManager.CreateAsync(user, "StrongPassword1!");
        created.Succeeded.ShouldBeTrue();
        await context.SaveChangesAsync(cancellationToken);
        var userId = user.Id;

        var migration = new RemoveNonPermissionClaims();
        foreach (var operation in migration.UpOperations.Cast<SqlOperation>())
        {
            await context.Database.ExecuteSqlRawAsync(operation.Sql, cancellationToken);
            await context.Database.ExecuteSqlRawAsync(operation.Sql, cancellationToken);
        }

        context.ChangeTracker.Clear();
        var userClaims = await context.Set<ApplicationUserClaim>()
            .Where(claim => claim.UserId == userId).ToArrayAsync(cancellationToken);
        userClaims.Select(claim => claim.ClaimType).ShouldBe(["permission", " PERMISSION "], ignoreOrder: true);
        userClaims.Select(claim => claim.ClaimValue)
            .ShouldBe(["identity.users.read", "retired.permission"], ignoreOrder: true);
        var roleClaims = await context.Set<ApplicationRoleClaim>().ToArrayAsync(cancellationToken);
        roleClaims.ShouldAllBe(claim => claim.ClaimType!.Trim().Equals("permission", StringComparison.OrdinalIgnoreCase));
        roleClaims.Count(claim => claim.ClaimType == "permission").ShouldBe(32);
        roleClaims.ShouldContain(claim => claim.ClaimType == " PERMISSION " && claim.ClaimValue == "retired.permission");
    }
}
