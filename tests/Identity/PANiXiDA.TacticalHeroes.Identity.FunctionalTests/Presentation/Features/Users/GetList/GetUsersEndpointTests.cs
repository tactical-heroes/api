using System.Net.Http.Headers;

using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Users.GetList;

public sealed class GetUsersEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "GET users should return a filtered page when email substring matches")]
    public async Task GetUsers_Should_ReturnFilteredPage_When_EmailSubstringMatches()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new UserApiTestClient(Fixture);
        await client.CreateAsync(
            UserApiTestClient.CreateDefaultRequest("first@example.com", "first-hero"),
            cancellationToken);
        var secondUser = await client.CreateAsync(
            UserApiTestClient.CreateDefaultRequest("second@example.com", "second-hero"),
            cancellationToken);

        var response = await client.GetListAsync(
            email: "ECOND",
            pageNumber: 1,
            pageSize: 20,
            cancellationToken: cancellationToken);

        response.TotalCount.ShouldBe(1);
        var user = response.Items.ShouldHaveSingleItem();
        user.Id.ShouldBe(secondUser.Id);
        user.Email.ShouldBe("second@example.com");
    }

    [Theory(DisplayName = "GET users should require read permission when authenticated")]
    [InlineData("unrelated", HttpStatusCode.Forbidden)]
    [InlineData("identity.users.read", HttpStatusCode.OK)]
    public async Task GetUsers_Should_RequireReadPermission_When_Authenticated(
        string permission, HttpStatusCode expectedStatus)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/users?pageNumber=1&pageSize=20");
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
