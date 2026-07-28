using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Persistence.Features.Roles.Read;

public sealed class RolesReadRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "GetDetailsByIdAsync should return role details when role exists")]
    public async Task GetDetailsByIdAsync_Should_ReturnDetails_When_RoleExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var roleId = await AddRoleAsync("admin", cancellationToken);

        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRolesReadRepository>();
        var details = await repository.GetDetailsByIdAsync(roleId, cancellationToken);

        details.ShouldNotBeNull();
        details.Name.ShouldBe("admin");
    }

    [Fact(DisplayName = "GetPagedAsync should return roles sorted by name when roles exist")]
    public async Task GetPagedAsync_Should_ReturnSortedPage_When_RolesExist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await AddRoleAsync("viewer", cancellationToken);
        await AddRoleAsync("admin", cancellationToken);

        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRolesReadRepository>();
        var page = await repository.GetPagedAsync(
            new PaginationParameters(1, 20),
            cancellationToken);

        page.TotalCount.ShouldBe(2);
        page.Items.Select(item => item.Name).ShouldBe(["admin", "viewer"]);
    }

    [Fact(DisplayName = "ExistsByIdAsync should return true for an existing role when role exists")]
    public async Task ExistsByIdAsync_Should_ReturnTrue_When_RoleExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var roleId = await AddRoleAsync("exists-role", cancellationToken);

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

        await AddRoleAsync("any-role", cancellationToken);

        await using var populatedScope = Fixture.CreateScope();
        var populatedRepository =
            populatedScope.ServiceProvider.GetRequiredService<IRolesReadRepository>();
        (await populatedRepository.AnyAsync(cancellationToken)).ShouldBeTrue();
    }

    private async Task<Guid> AddRoleAsync(
        string name,
        CancellationToken cancellationToken)
    {
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRolesWriteRepository>();
        var result = await repository.AddAsync(name, [], cancellationToken);

        result.IsSuccess.ShouldBeTrue();

        return result.Value;
    }
}
