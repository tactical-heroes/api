using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Persistence.Core.Migrations;

public sealed class SeedRolesAndClaimsTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "Seed migration should initialize roles and claims when roles are missing")]
    public async Task Up_Should_CreateRolesAndClaims_When_RolesAreMissing()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        await Fixture.SeedRolesAsync(cancellationToken);

        await using var scope = Fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var roles = await dbContext.Set<ApplicationRole>()
            .Include(role => role.Claims)
            .AsNoTracking()
            .ToArrayAsync(cancellationToken);
        roles.Select(role => role.Name).ShouldBe(["Developer", "Moderator", "Player"], ignoreOrder: true);
        roles.ShouldAllBe(role => role.Id != Guid.Empty && role.CreatedAt != default && role.UpdatedAt != default);
        roles.ShouldAllBe(role => string.Equals(
            role.NormalizedName, role.Name!.ToUpperInvariant(), StringComparison.Ordinal));
        roles.Single(role => role.IsDefault).Name.ShouldBe("Player");
        var developer = roles.Single(role => role.Name == "Developer");
        developer.Claims.Count(claim => claim.ClaimType == "permission").ShouldBe(20);
        developer.Claims.Where(claim => claim.ClaimType == "section").Select(claim => claim.ClaimValue)
            .ShouldBe(["identity", "compendium"], ignoreOrder: true);
        var moderator = roles.Single(role => role.Name == "Moderator");
        moderator.Claims.Count.ShouldBe(13);
        moderator.Claims.ShouldAllBe(claim => claim.ClaimValue!.StartsWith("compendium", StringComparison.Ordinal));
        roles.Single(role => role.Name == "Player").Claims.ShouldBeEmpty();
        dbContext.Database.HasPendingModelChanges().ShouldBeFalse();
    }

    [Fact(DisplayName = "Seed migration should preserve existing IDs and custom claims when roles already exist")]
    public async Task Up_Should_PreserveIdsAndCustomClaims_When_RolesAlreadyExist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var id = Guid.CreateVersion7();
        dbContext.Set<ApplicationRole>().Add(new ApplicationRole
        {
            Id = id,
            Name = "developer",
            NormalizedName = "DEVELOPER",
            IsDefault = true,
            Claims = [new ApplicationRoleClaim { ClaimType = "custom", ClaimValue = "kept" }]
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        await Fixture.SeedRolesAsync(cancellationToken);
        await Fixture.SeedRolesAsync(cancellationToken);

        dbContext.ChangeTracker.Clear();
        var roles = await dbContext.Set<ApplicationRole>().Include(role => role.Claims).ToArrayAsync(cancellationToken);
        roles.Length.ShouldBe(3);
        var developer = roles.Single(role => role.Id == id);
        developer.Name.ShouldBe("developer");
        developer.IsDefault.ShouldBeTrue();
        developer.Claims.Count.ShouldBe(23);
        developer.Claims.ShouldContain(claim => claim.ClaimType == "custom" && claim.ClaimValue == "kept");
        roles.Single(role => role.Name == "Player").IsDefault.ShouldBeFalse();
    }

    [Fact(DisplayName = "Default role index should reject a second default when one already exists")]
    public async Task SaveChangesAsync_Should_RejectSecondDefault_When_DefaultExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await Fixture.SeedRolesAsync(cancellationToken);
        await using var scope = Fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var moderator = await dbContext.Set<ApplicationRole>()
            .SingleAsync(role => role.Name == "Moderator", cancellationToken);
        moderator.IsDefault = true;

        await Should.ThrowAsync<DbUpdateException>(() => dbContext.SaveChangesAsync(cancellationToken));
    }
}
