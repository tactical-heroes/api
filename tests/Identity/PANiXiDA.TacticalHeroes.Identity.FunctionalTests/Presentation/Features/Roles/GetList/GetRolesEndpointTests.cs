using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Create;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Roles.GetList;

public sealed class GetRolesEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "GET roles should return a sorted page from PostgreSQL")]
    public async Task GetRoles_Should_ReturnSortedPage_When_RolesExist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new RoleApiTestClient(Fixture);
        await client.CreateAsync(cancellationToken, new CreateRoleRequest("viewer", []));
        await client.CreateAsync(cancellationToken, new CreateRoleRequest("admin", []));

        var response = await client.GetListAsync(cancellationToken);

        response.TotalCount.ShouldBe(2);
        response.Items.Select(role => role.Name).ShouldBe(["admin", "viewer"]);
    }
}
