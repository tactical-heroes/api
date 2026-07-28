using System.Security.Claims;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Persistence.Features.Roles.Write;

public sealed class RolesRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "AddAsync should persist a valid role")]
    public async Task AddAsync_Should_PersistRole_When_CommandIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var roleId = await AddRoleAsync(
            " ADMIN ",
            [new Claim("permission", "heroes.read")],
            cancellationToken);

        await using var scope = Fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var role = await dbContext.Set<ApplicationRole>()
            .Include(item => item.Claims)
            .AsNoTracking()
            .SingleAsync(item => item.Id == roleId, cancellationToken);

        role.Name.ShouldBe("admin");
        role.Claims.ShouldHaveSingleItem().ClaimValue.ShouldBe("heroes.read");
    }

    [Fact(DisplayName = "UpdateAsync should persist role changes")]
    public async Task UpdateAsync_Should_PersistChanges_When_RoleExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var roleId = await AddRoleAsync(
            "admin",
            [new Claim("permission", "heroes.read")],
            cancellationToken);

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRolesWriteRepository>();
            (await repository.UpdateAsync(
                roleId,
                "manager",
                [new Claim("permission", "heroes.manage")],
                cancellationToken)).IsSuccess.ShouldBeTrue();
        }

        await using var verificationScope = Fixture.CreateScope();
        var dbContext =
            verificationScope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var role = await dbContext.Set<ApplicationRole>()
            .Include(item => item.Claims)
            .AsNoTracking()
            .SingleAsync(item => item.Id == roleId, cancellationToken);

        role.Name.ShouldBe("manager");
        role.Claims.ShouldHaveSingleItem().ClaimValue.ShouldBe("heroes.manage");
    }

    [Fact(DisplayName = "DeleteAsync should remove an existing role")]
    public async Task DeleteAsync_Should_RemoveRole_When_RoleExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var roleId = await AddRoleAsync("delete-role", [], cancellationToken);

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRolesWriteRepository>();
            (await repository.DeleteAsync(roleId, cancellationToken)).IsSuccess.ShouldBeTrue();
        }

        await using var verificationScope = Fixture.CreateScope();
        var dbContext =
            verificationScope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        (await dbContext.Set<ApplicationRole>()
                .AnyAsync(item => item.Id == roleId, cancellationToken))
            .ShouldBeFalse();
    }

    [Fact(DisplayName = "Roles repository should return conflict for a duplicate role")]
    public async Task AddAsync_Should_ReturnConflict_When_RoleNameAlreadyExists()
    {
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRolesWriteRepository>();
        var cancellationToken = TestContext.Current.CancellationToken;

        (await repository.AddAsync("admin", [], cancellationToken)).IsSuccess.ShouldBeTrue();
        var result = await repository.AddAsync("ADMIN", [], cancellationToken);

        result.Errors.ShouldContain(error => error.Type == ErrorType.Conflict);
    }

    private async Task<Guid> AddRoleAsync(
        string name,
        IReadOnlyCollection<Claim> claims,
        CancellationToken cancellationToken)
    {
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRolesWriteRepository>();
        var result = await repository.AddAsync(
            name,
            claims,
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();

        return result.Value;
    }
}
