using System.Net.Http.Headers;

using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Roles.GetList;

[Collection(FunctionalTestCollectionDefinition.Name)]
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
        response.Items.Select(role => role.Name).ShouldBe(["Developer", "Moderator", "Player"]);
    }

    [Theory(DisplayName = "GET roles should require read permission when authenticated")]
    [InlineData("unrelated", HttpStatusCode.Forbidden)]
    [InlineData("identity.users.create", HttpStatusCode.Forbidden)]
    [InlineData("identity.users.update", HttpStatusCode.Forbidden)]
    [InlineData("identity.roles.read", HttpStatusCode.OK)]
    public async Task GetRoles_Should_RequireReadPermission_When_Authenticated(
        string permission, HttpStatusCode expectedStatus)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/roles?pageNumber=1&pageSize=20");
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await OAuthServiceAccessTokenTestHelper.IssueAccessTokenAsync(
                Fixture,
                [permission],
                cancellationToken));

        using var response = await Client.SendAsync(request, cancellationToken);

        response.StatusCode.ShouldBe(expectedStatus, await response.Content.ReadAsStringAsync(cancellationToken));
    }
}
