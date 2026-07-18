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
    [Fact(DisplayName = "Roles repository should persist, update, and delete role state")]
    public async Task Repository_Should_PersistUpdateAndDelete_When_CommandsAreValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        Guid roleId;

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRolesWriteRepository>();
            var result = await repository.AddAsync(
                " ADMIN ",
                [new Claim("permission", "heroes.read")],
                cancellationToken);

            result.IsSuccess.ShouldBeTrue();
            roleId = result.Value;
        }

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRolesWriteRepository>();
            (await repository.UpdateAsync(
                roleId,
                "manager",
                [new Claim("permission", "heroes.manage")],
                cancellationToken)).IsSuccess.ShouldBeTrue();
        }

        await using (var scope = Fixture.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
            var role = await dbContext.Set<ApplicationRole>()
                .Include(item => item.Claims)
                .AsNoTracking()
                .SingleAsync(item => item.Id == roleId, cancellationToken);

            role.Name.ShouldBe("manager");
            role.Claims.ShouldHaveSingleItem().ClaimValue.ShouldBe("heroes.manage");
        }

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRolesWriteRepository>();
            (await repository.DeleteAsync(roleId, cancellationToken)).IsSuccess.ShouldBeTrue();
        }

        await using (var scope = Fixture.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
            (await dbContext.Set<ApplicationRole>()
                    .AnyAsync(item => item.Id == roleId, cancellationToken))
                .ShouldBeFalse();
        }
    }

    [Fact(DisplayName = "Roles repository should return conflict for a duplicate role")]
    public async Task Repository_Should_ReturnConflict_When_RoleNameAlreadyExists()
    {
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRolesWriteRepository>();
        var cancellationToken = TestContext.Current.CancellationToken;

        (await repository.AddAsync("admin", [], cancellationToken)).IsSuccess.ShouldBeTrue();
        var result = await repository.AddAsync("ADMIN", [], cancellationToken);

        result.Errors.ShouldContain(error => error.Type == ErrorType.Conflict);
    }
}
