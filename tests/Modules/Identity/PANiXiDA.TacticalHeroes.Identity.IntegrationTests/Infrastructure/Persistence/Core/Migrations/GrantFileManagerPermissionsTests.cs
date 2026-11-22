using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core.Migrations;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Persistence.Core.Migrations;

public sealed class GrantFileManagerPermissionsTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Theory(DisplayName = "Up should grant scoped file permissions when default roles exist")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Up_Should_GrantScopedFilePermissions_When_DefaultRolesExist(bool alreadyGranted)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var developerId = Guid.CreateVersion7();
        context.Set<ApplicationRole>().Add(new ApplicationRole
        {
            Id = developerId,
            Name = "developer",
            NormalizedName = "DEVELOPER",
            Claims = [new ApplicationRoleClaim { ClaimType = "custom", ClaimValue = "kept" }]
        });
        await context.SaveChangesAsync(cancellationToken);
        await Fixture.SeedRolesAsync(cancellationToken);
        var player = await context.Set<ApplicationRole>()
            .SingleAsync(role => role.NormalizedName == "PLAYER", cancellationToken);
        if (alreadyGranted)
        {
            player.Claims.Add(new ApplicationRoleClaim
            {
                ClaimType = "permission",
                ClaimValue = Permissions.FileManager.PersonalFiles.ReadOwn.Code
            });
            await context.SaveChangesAsync(cancellationToken);
        }

        var migration = new GrantFileManagerPermissions();
        foreach (var operation in migration.UpOperations.Cast<SqlOperation>())
        {
            await context.Database.ExecuteSqlRawAsync(operation.Sql, cancellationToken);
            await context.Database.ExecuteSqlRawAsync(operation.Sql, cancellationToken);
        }

        context.ChangeTracker.Clear();
        var roles = await context.Set<ApplicationRole>().Include(role => role.Claims)
            .AsNoTracking().ToArrayAsync(cancellationToken);
        var developer = roles.Single(role => role.NormalizedName == "DEVELOPER");
        developer.Id.ShouldBe(developerId);
        developer.Claims.ShouldContain(claim => claim.ClaimType == "custom" && claim.ClaimValue == "kept");
        developer.Claims.Count(claim => claim.ClaimValue!.StartsWith("identity.", StringComparison.Ordinal)).ShouldBe(8);
        var developerFilePermissions = developer.Claims
            .Where(claim => claim.ClaimType == "permission"
                && claim.ClaimValue!.StartsWith("file-manager.", StringComparison.Ordinal))
            .Select(claim => claim.ClaimValue)
            .ToArray();
        developerFilePermissions.Length.ShouldBe(12);
        developerFilePermissions.ShouldBe(
            Permissions.All.Where(permission => permission.ModuleCode == Permissions.FileManager.Code)
                .Select(permission => permission.Code),
            ignoreOrder: true);
        roles.Single(role => role.NormalizedName == "PLAYER").Claims.Select(claim => claim.ClaimValue).ShouldBe(
        [
            "file-manager.personal-files.create.own",
            "file-manager.personal-files.read.own",
            "file-manager.personal-files.update.own",
            "file-manager.personal-files.delete.own",
            "file-manager.avatars.read"
        ],
        ignoreOrder: true);
        roles.Single(role => role.NormalizedName == "MODERATOR").Claims.ShouldAllBe(claim =>
            !claim.ClaimValue!.StartsWith("file-manager.", StringComparison.Ordinal));
        context.Database.HasPendingModelChanges().ShouldBeFalse();
    }
}
