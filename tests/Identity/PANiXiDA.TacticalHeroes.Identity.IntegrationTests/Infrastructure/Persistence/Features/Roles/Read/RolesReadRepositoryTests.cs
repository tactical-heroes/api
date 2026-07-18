using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Persistence.Features.Roles.Read;

public sealed class RolesReadRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "Roles read repository should return persisted details and a sorted page")]
    public async Task Repository_Should_ReturnDetailsAndSortedPage_When_RolesExist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        Guid adminRoleId;

        await using (var scope = Fixture.CreateScope())
        {
            var writeRepository = scope.ServiceProvider.GetRequiredService<IRolesWriteRepository>();
            (await writeRepository.AddAsync("viewer", [], cancellationToken)).IsSuccess.ShouldBeTrue();
            adminRoleId = (await writeRepository.AddAsync("admin", [], cancellationToken)).Value;
        }

        await using var verificationScope = Fixture.CreateScope();
        var readRepository = verificationScope.ServiceProvider.GetRequiredService<IRolesReadRepository>();
        var details = await readRepository.GetDetailsByIdAsync(adminRoleId, cancellationToken);
        var page = await readRepository.GetPagedAsync(
            new PaginationParameters(1, 20),
            cancellationToken);

        details.ShouldNotBeNull();
        details.Name.ShouldBe("admin");
        page.TotalCount.ShouldBe(2);
        page.Items.Select(item => item.Name).ShouldBe(["admin", "viewer"]);
        (await readRepository.ExistsByIdAsync(adminRoleId, cancellationToken)).ShouldBeTrue();
    }
}
