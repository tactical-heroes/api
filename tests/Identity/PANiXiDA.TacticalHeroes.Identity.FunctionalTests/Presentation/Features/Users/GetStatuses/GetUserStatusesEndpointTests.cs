using System.Net.Http.Headers;

using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Users.GetStatuses;

public sealed class GetUserStatusesEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "GET user statuses should return all supported statuses when authorized")]
    public async Task GetUserStatuses_Should_ReturnAllStatuses_When_Authorized()
    {
        var client = new UserApiTestClient(Fixture);

        var response = await client.GetStatusesAsync(TestContext.Current.CancellationToken);

        response.ShouldContain(status =>
            status.Id == UserStatus.Active.Id
                && status.Name == UserStatus.Active.Name);
        response.ShouldContain(status =>
            status.Id == UserStatus.Blocked.Id
                && status.Name == UserStatus.Blocked.Name);
    }

    [Theory(DisplayName = "GET user statuses should require read permission when authenticated")]
    [InlineData("unrelated", HttpStatusCode.Forbidden)]
    [InlineData("identity.users.create", HttpStatusCode.Forbidden)]
    [InlineData("identity.users.update", HttpStatusCode.Forbidden)]
    [InlineData("identity.users.read", HttpStatusCode.OK)]
    public async Task GetUserStatuses_Should_RequireReadPermission_When_Authenticated(
        string permission, HttpStatusCode expectedStatus)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/users/statuses");
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
