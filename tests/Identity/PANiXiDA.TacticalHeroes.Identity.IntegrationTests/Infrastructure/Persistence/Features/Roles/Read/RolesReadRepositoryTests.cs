using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Roles;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Persistence.Features.Roles.Read;

public sealed class RolesReadRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "GetDetailsByIdAsync should return role details when role exists")]
    public async Task GetDetailsByIdAsync_Should_ReturnDetails_When_RoleExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var roleId = await AddRoleAsync("Developer", cancellationToken);

        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRolesReadRepository>();
        var details = await repository.GetDetailsByIdAsync(roleId, cancellationToken);

        details.ShouldNotBeNull();
        details.Name.ShouldBe("developer");
    }

    [Fact(DisplayName = "GetPageAsync should return roles sorted by name when roles exist")]
    public async Task GetPageAsync_Should_ReturnSortedPage_When_RolesExist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await AddRoleAsync("Player", cancellationToken);
        await AddRoleAsync("Developer", cancellationToken);

        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRolesReadRepository>();
        var page = await repository.GetPageAsync(
            new PaginationParameters(1, 20),
            SortingParameters.None,
            cancellationToken);

        page.TotalCount.ShouldBe(2);
        page.Items.Select(item => item.Name).ShouldBe(["developer", "player"]);
    }

    [Fact(DisplayName = "ExistsByIdAsync should return true for an existing role when role exists")]
    public async Task ExistsByIdAsync_Should_ReturnTrue_When_RoleExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var roleId = await AddRoleAsync("Moderator", cancellationToken);

        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRolesReadRepository>();

        (await repository.ExistsByIdAsync(roleId, cancellationToken)).ShouldBeTrue();
    }

    [Fact(DisplayName = "AnyAsync should reflect whether roles exist when called")]
    public async Task AnyAsync_Should_ReflectWhetherRolesExist_When_Called()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        await using (var emptyScope = Fixture.CreateScope())
        {
            var emptyRepository =
                emptyScope.ServiceProvider.GetRequiredService<IRolesReadRepository>();
            (await emptyRepository.AnyAsync(cancellationToken)).ShouldBeFalse();
        }

        await AddRoleAsync("Moderator", cancellationToken);

        await using var populatedScope = Fixture.CreateScope();
        var populatedRepository =
            populatedScope.ServiceProvider.GetRequiredService<IRolesReadRepository>();
        (await populatedRepository.AnyAsync(cancellationToken)).ShouldBeTrue();
    }

    [Fact(DisplayName = "GetDefaultIdAsync should return null when default is missing")]
    public async Task GetDefaultIdAsync_Should_ReturnNull_When_DefaultIsMissing()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await AddRoleAsync("Moderator", cancellationToken);
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRolesReadRepository>();

        var result = await repository.GetDefaultIdAsync(cancellationToken);

        result.ShouldBeNull();
    }

    [Fact(DisplayName = "GetDefaultIdAsync should return the configured role id when default exists")]
    public async Task GetDefaultIdAsync_Should_ReturnConfiguredRoleId_When_DefaultExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await AddRoleAsync("Moderator", cancellationToken);
        await using var scope = Fixture.CreateScope();
        var writeRepository = scope.ServiceProvider.GetRequiredService<IRolesRepository>();
        var roleId = Guid.CreateVersion7();
        var created = await writeRepository.AddAsync(
            IntegrationTestData.CreateRole(
                roleId, "CustomDefault", [("permission", "compendium.heroes.read")], isDefault: true),
            cancellationToken);
        created.IsSuccess.ShouldBeTrue();
        await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(IdentityWriteDbContext))
            .SaveChangesAsync(cancellationToken);
        var readRepository = scope.ServiceProvider.GetRequiredService<IRolesReadRepository>();

        var result = await readRepository.GetDefaultIdAsync(cancellationToken);

        result.ShouldBe(roleId);
    }

    private async Task<Guid> AddRoleAsync(
        string name,
        CancellationToken cancellationToken)
    {
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRolesRepository>();
        var result = await repository.AddAsync(
            IntegrationTestData.CreateRole(Guid.CreateVersion7(), name, []),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(IdentityWriteDbContext))
            .SaveChangesAsync(cancellationToken);

        return result.Value;
    }
}
