using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Roles;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Persistence.Features.Roles.Read;

public sealed class RolesReadRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "GetSelectOptionsAsync should return empty options when no roles exist")]
    public async Task GetSelectOptionsAsync_Should_ReturnEmptyOptions_When_NoRolesExist()
    {
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRolesReadRepository>();

        var options = await repository.GetSelectOptionsAsync(TestContext.Current.CancellationToken);

        options.ShouldBeEmpty();
    }

    [Fact(DisplayName = "GetSelectOptionsAsync should return sorted options with default flag when roles exist")]
    public async Task GetSelectOptionsAsync_Should_ReturnSortedOptionsWithDefaultFlag_When_RolesExist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var playerId = await AddRoleAsync("Player", cancellationToken, isDefault: true);
        var developerId = await AddRoleAsync("Developer", cancellationToken);
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRolesReadRepository>();

        var options = await repository.GetSelectOptionsAsync(cancellationToken);

        options.Select(option => option.Name).ShouldBe(["developer", "player"]);
        options.Select(option => option.Id).ShouldBe([developerId, playerId]);
        options.Select(option => option.IsDefault).ShouldBe([false, true]);
    }

    [Fact(DisplayName = "GetSelectOptionsAsync should return all options when more than one list page exists")]
    public async Task GetSelectOptionsAsync_Should_ReturnAllOptions_When_MoreThanOneListPageExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        for (int index = 0; index < 101; index++)
        {
            await AddRoleAsync($"Role {index:D3}", cancellationToken);
        }
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRolesReadRepository>();

        var options = await repository.GetSelectOptionsAsync(cancellationToken);

        options.Count.ShouldBe(101);
        options[0].Name.ShouldBe("role 000");
        options[^1].Name.ShouldBe("role 100");
    }

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
        var readRepository = scope.ServiceProvider.GetRequiredService<IRolesReadRepository>();

        var result = await readRepository.GetDefaultIdAsync(cancellationToken);

        result.ShouldBe(roleId);
    }

    private async Task<Guid> AddRoleAsync(
        string name,
        CancellationToken cancellationToken,
        bool isDefault = false)
    {
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRolesRepository>();
        var result = await repository.AddAsync(
            IntegrationTestData.CreateRole(Guid.CreateVersion7(), name, [], isDefault),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();

        return result.Value;
    }
}
