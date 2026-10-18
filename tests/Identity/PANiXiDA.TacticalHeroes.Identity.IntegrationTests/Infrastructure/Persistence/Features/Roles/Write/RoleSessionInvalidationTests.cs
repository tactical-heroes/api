using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;

using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Roles;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Persistence.Features.Roles.Write;

public sealed class RoleSessionInvalidationTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Theory(DisplayName = "Role writes should invalidate affected sessions when role changes vary")]
    [InlineData("claims")]
    [InlineData("name")]
    [InlineData("delete")]
    [InlineData("unchanged")]
    [InlineData("default")]
    public async Task SaveAsync_Should_InvalidateAffectedSessions_When_RoleChangesVary(string change)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRolesRepository>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var tokenManager = scope.ServiceProvider.GetRequiredService<IOpenIddictTokenManager>();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var roleId = Guid.CreateVersion7();
        var role = IntegrationTestData.CreateRole(roleId, "original", [("permission", "compendium.heroes.read")]);
        (await repository.AddAsync(role, cancellationToken)).IsSuccess.ShouldBeTrue();
        var stamps = new Dictionary<Guid, string?>();
        var affected = new HashSet<Guid>();
        for (var index = 0; index < 3; index++)
        {
            var user = new ApplicationUser
            {
                Id = Guid.CreateVersion7(),
                Email = $"user{index}@example.com",
                UserName = $"user{index}",
                EmailConfirmed = true,
                Roles = index < 2 ? [new ApplicationUserRole { RoleId = roleId }] : []
            };
            (await userManager.CreateAsync(user, "StrongPassword1!")).Succeeded.ShouldBeTrue();
            stamps.Add(user.Id, user.SecurityStamp);
            if (index < 2)
            {
                affected.Add(user.Id);
            }
            foreach (var type in new[]
            {
                OpenIddictConstants.TokenTypeIdentifiers.AccessToken,
                OpenIddictConstants.TokenTypeIdentifiers.RefreshToken
            })
            {
                await tokenManager.CreateAsync(
                    new OpenIddictTokenDescriptor
                    {
                        Subject = user.Id.ToString(),
                        Status = OpenIddictConstants.Statuses.Valid,
                        Type = type
                    },
                    cancellationToken);
            }
        }
        var target = IntegrationTestData.CreateRole(
            id: roleId,
            name: change == "name" ? "renamed" : "original",
            claims: [("permission", change == "claims" ? "compendium.heroes.update" : "compendium.heroes.read")],
            isDefault: change == "default");

        var result = change == "delete"
            ? await repository.DeleteAsync(roleId, cancellationToken)
            : await repository.UpdateAsync(target, cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        var users = await dbContext.Set<ApplicationUser>()
            .AsNoTracking()
            .ToArrayAsync(cancellationToken);
        var tokens = await dbContext.Set<OpenIddictEntityFrameworkCoreToken<Guid>>()
            .AsNoTracking()
            .ToArrayAsync(cancellationToken);
        tokens.Length.ShouldBe(6);
        var authorizationChanged = change is "claims" or "name" or "delete";
        foreach (var user in users)
        {
            var revoked = authorizationChanged && affected.Contains(user.Id);
            (user.SecurityStamp != stamps[user.Id]).ShouldBe(revoked);
            tokens.Where(token => token.Subject == user.Id.ToString()).ShouldAllBe(token => token.Status == (revoked
                ? OpenIddictConstants.Statuses.Revoked
                : OpenIddictConstants.Statuses.Valid));
        }
        if (change == "delete")
        {
            (await dbContext.Set<ApplicationUserRole>().AnyAsync(cancellationToken)).ShouldBeFalse();
        }
    }
}
