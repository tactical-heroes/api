namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Roles.GetList;

public sealed class GetRolesEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "GET roles should return a sorted page from PostgreSQL when roles exist")]
    public async Task GetRoles_Should_ReturnSortedPage_When_RolesExist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new RoleApiTestClient(Fixture);

        var response = await client.GetListAsync(cancellationToken);

        response.TotalCount.ShouldBe(3);
        response.Items.Select(role => role.Name).ShouldBe(["developer", "moderator", "player"]);
    }
}
