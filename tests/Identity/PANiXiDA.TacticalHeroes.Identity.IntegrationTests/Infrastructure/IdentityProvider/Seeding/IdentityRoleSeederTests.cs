using System.Security.Claims;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Seeding;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.IdentityProvider.Seeding;

public sealed class IdentityRoleSeederTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "Role seeding should create predefined roles when roles are missing")]
    public async Task SeedAsync_Should_CreatePredefinedRoles_When_RolesAreMissing()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IdentityRoleSeeder>();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();

        await seeder.SeedAsync(cancellationToken);

        var roles = await dbContext.Set<ApplicationRole>()
            .Include(role => role.Claims)
            .AsNoTracking()
            .ToArrayAsync(cancellationToken);
        roles.Select(role => role.Name).ShouldBe(["Developer", "Moderator", "Player"], ignoreOrder: true);
        roles.ShouldAllBe(role => role.Id != Guid.Empty && role.CreatedAt != default && role.UpdatedAt != default);
        roles.ShouldAllBe(role => string.Equals(
            role.NormalizedName, role.Name!.ToUpperInvariant(), StringComparison.Ordinal));
        roles.ShouldAllBe(role => role.Claims.Count == 0);
    }

    [Fact(DisplayName = "Role seeding should preserve IDs and claims when roles already exist")]
    public async Task SeedAsync_Should_PreserveIdsAndClaims_When_RolesAlreadyExist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IdentityRoleSeeder>();
        var manager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        await seeder.SeedAsync(cancellationToken);
        var role = await manager.FindByNameAsync(RoleName.Developer.Name);
        role.ShouldNotBeNull();
        var id = role.Id;
        (await manager.AddClaimAsync(role, new Claim("permission", "identity.users.read"))).Succeeded.ShouldBeTrue();

        await seeder.SeedAsync(cancellationToken);

        var existing = await manager.FindByNameAsync(RoleName.Developer.Name);
        existing.ShouldNotBeNull();
        existing.Id.ShouldBe(id);
        (await manager.GetClaimsAsync(existing)).ShouldHaveSingleItem().Value.ShouldBe("identity.users.read");
        (await manager.Roles.CountAsync(cancellationToken)).ShouldBe(3);
    }

    [Fact(DisplayName = "Role seeding should stop when cancellation is requested")]
    public async Task SeedAsync_Should_Throw_When_CancellationIsRequested()
    {
        await using var scope = Fixture.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IdentityRoleSeeder>();
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var exception = await Should.ThrowAsync<OperationCanceledException>(() => seeder.SeedAsync(cancellation.Token));

        exception.CancellationToken.ShouldBe(cancellation.Token);
    }

    [Fact(DisplayName = "Role seeding should fail startup when role creation fails")]
    public async Task SeedAsync_Should_Throw_When_RoleCreationFails()
    {
        await using var scope = Fixture.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IdentityRoleSeeder>();
        var manager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        manager.RoleValidators.Add(new RejectingRoleValidator());

        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => seeder.SeedAsync(TestContext.Current.CancellationToken));

        exception.Message.ShouldContain("Rejected role");
    }

    private sealed class RejectingRoleValidator : IRoleValidator<ApplicationRole>
    {
        public Task<IdentityResult> ValidateAsync(RoleManager<ApplicationRole> manager, ApplicationRole role) =>
            Task.FromResult(IdentityResult.Failed(
                new IdentityError { Code = "Rejected", Description = "Rejected role" }));
    }
}
