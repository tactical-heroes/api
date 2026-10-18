using System.Security.Claims;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Roles;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Persistence.Features.Roles.Write;

public sealed class RolesRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "AddAsync should persist a valid role when command is valid")]
    public async Task AddAsync_Should_PersistRole_When_CommandIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var roleId = await AddRoleAsync(
            " Developer ",
            [new Claim("permission", "heroes.read")],
            cancellationToken);

        await using var scope = Fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var role = await dbContext.Set<ApplicationRole>()
            .Include(item => item.Claims)
            .AsNoTracking()
            .SingleAsync(item => item.Id == roleId, cancellationToken);

        role.Name.ShouldBe("Developer");
        role.Claims.ShouldHaveSingleItem().ClaimValue.ShouldBe("heroes.read");
    }

    [Fact(DisplayName = "UpdateAsync should persist role changes when role exists")]
    public async Task UpdateAsync_Should_PersistChanges_When_RoleExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var roleId = await AddRoleAsync(
            "Developer",
            [new Claim("permission", "heroes.read")],
            cancellationToken);

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRolesRepository>();
            (await repository.UpdateAsync(
                IntegrationTestData.CreateRole(
                    id: roleId,
                    name: "Moderator",
                    claims: ((Claim[])[new Claim("permission", "heroes.manage")]).Select(
                        claim => (claim.Type, claim.Value))),
                cancellationToken))
                .IsSuccess
                .ShouldBeTrue();
        }

        await using var verificationScope = Fixture.CreateScope();
        var dbContext =
            verificationScope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var role = await dbContext.Set<ApplicationRole>()
            .Include(item => item.Claims)
            .AsNoTracking()
            .SingleAsync(item => item.Id == roleId, cancellationToken);

        role.Name.ShouldBe("Moderator");
        role.Claims.ShouldHaveSingleItem().ClaimValue.ShouldBe("heroes.manage");
    }

    [Fact(DisplayName = "DeleteAsync should remove an existing role when role exists")]
    public async Task DeleteAsync_Should_RemoveRole_When_RoleExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var roleId = await AddRoleAsync("Player", [], cancellationToken);

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRolesRepository>();
            (await repository.DeleteAsync(roleId, cancellationToken)).IsSuccess.ShouldBeTrue();
        }

        await using var verificationScope = Fixture.CreateScope();
        var dbContext =
            verificationScope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        (await dbContext.Set<ApplicationRole>()
            .AnyAsync(item => item.Id == roleId, cancellationToken))
            .ShouldBeFalse();
    }

    [Fact(DisplayName = "Roles repository should return conflict for a duplicate role when role name already exists")]
    public async Task AddAsync_Should_ReturnConflict_When_RoleNameAlreadyExists()
    {
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRolesRepository>();
        var cancellationToken = TestContext.Current.CancellationToken;

        (await repository.AddAsync(
            IntegrationTestData.CreateRole(
                id: Guid.CreateVersion7(),
                name: "Developer",
                claims: []),
            cancellationToken))
            .IsSuccess
            .ShouldBeTrue();
        var result = await repository.AddAsync(
            IntegrationTestData.CreateRole(
                id: Guid.CreateVersion7(),
                name: "Developer",
                claims: []),
            cancellationToken);

        result.Errors.ShouldContain(error => error.Type == ErrorType.Conflict);
    }

    [Theory(DisplayName = "Role persistence should switch the default when different role is selected")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SaveAsync_Should_SwitchDefault_When_DifferentRoleIsSelected(bool create)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await Fixture.SeedRolesAsync(cancellationToken);
        await using var scope = Fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var previous = await dbContext.Set<ApplicationRole>().SingleAsync(role => role.IsDefault, cancellationToken);
        var targetId = create
            ? Guid.CreateVersion7()
            : await dbContext.Set<ApplicationRole>()
                .Where(role => role.Name == "Moderator")
                .Select(role => role.Id)
                .SingleAsync(cancellationToken);
        var role = IntegrationTestData.CreateRole(targetId, "CustomDefault", [], isDefault: true);
        var repository = scope.ServiceProvider.GetRequiredService<IRolesRepository>();

        if (create)
        {
            (await repository.AddAsync(role, cancellationToken)).IsSuccess.ShouldBeTrue();
        }
        else
        {
            (await repository.UpdateAsync(role, cancellationToken)).IsSuccess.ShouldBeTrue();
        }

        var defaults = await dbContext.Set<ApplicationRole>()
            .AsNoTracking()
            .Where(item => item.IsDefault)
            .ToArrayAsync(cancellationToken);
        defaults.ShouldHaveSingleItem().Id.ShouldBe(targetId);
        previous.IsDefault.ShouldBeFalse();
        (await repository.DeleteAsync(previous.Id, cancellationToken)).IsSuccess.ShouldBeTrue();
    }

    [Theory(DisplayName = "Role persistence should retain the default when duplicate name rejects change")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SaveAsync_Should_RetainDefault_When_DuplicateNameRejectsChange(bool create)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await Fixture.SeedRolesAsync(cancellationToken);
        await using var scope = Fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var targetId = create
            ? Guid.CreateVersion7()
            : await dbContext.Set<ApplicationRole>()
                .Where(role => role.Name == "Moderator")
                .Select(role => role.Id)
                .SingleAsync(cancellationToken);
        var role = IntegrationTestData.CreateRole(targetId, "Developer", [], isDefault: true);
        var repository = scope.ServiceProvider.GetRequiredService<IRolesRepository>();

        var result = create
            ? (await repository.AddAsync(role, cancellationToken)).IsFailure
            : (await repository.UpdateAsync(role, cancellationToken)).IsFailure;

        result.ShouldBeTrue();
        var defaultName = await dbContext.Set<ApplicationRole>()
            .AsNoTracking()
            .Where(item => item.IsDefault)
            .Select(item => item.Name)
            .SingleAsync(cancellationToken);
        defaultName.ShouldBe("Player");
    }

    [Theory(DisplayName = "Role persistence should reject removal when current default would be removed")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SaveAsync_Should_ReturnConflict_When_CurrentDefaultWouldBeRemoved(bool delete)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await Fixture.SeedRolesAsync(cancellationToken);
        await using var scope = Fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var role = await dbContext.Set<ApplicationRole>().SingleAsync(item => item.IsDefault, cancellationToken);
        var repository = scope.ServiceProvider.GetRequiredService<IRolesRepository>();

        var result = delete
            ? await repository.DeleteAsync(role.Id, cancellationToken)
            : await repository.UpdateAsync(
                IntegrationTestData.CreateRole(role.Id, "Renamed", [], isDefault: false), cancellationToken);

        result.Errors.ShouldContain(error => error.Type == ErrorType.Conflict);
        var existing = await dbContext.Set<ApplicationRole>().AsNoTracking()
            .SingleAsync(item => item.Id == role.Id, cancellationToken);
        existing.IsDefault.ShouldBeTrue();
        existing.Name.ShouldBe("Player");
    }

    private async Task<Guid> AddRoleAsync(
        string name,
        IReadOnlyCollection<Claim> claims,
        CancellationToken cancellationToken)
    {
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRolesRepository>();
        var result = await repository.AddAsync(
            IntegrationTestData.CreateRole(
                id: Guid.CreateVersion7(),
                name: name,
                claims: claims.Select(claim => (claim.Type, claim.Value))),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();

        return result.Value;
    }
}
