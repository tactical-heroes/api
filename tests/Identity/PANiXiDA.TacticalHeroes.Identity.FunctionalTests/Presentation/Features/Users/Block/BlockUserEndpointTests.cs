using System.Net.Http.Headers;

using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Users.Block;

public sealed class BlockUserEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "POST user block should persist blocked status and revoke access when user exists")]
    public async Task BlockUser_Should_PersistBlockedStatusAndRevokeAccess_When_UserExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new UserApiTestClient(Fixture);
        var createdUser = await client.CreateAsync(cancellationToken);
        using var userClient = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var tokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            userClient,
            "hero@example.com",
            "StrongPassword1!",
            cancellationToken);
        userClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        using var initialResponse = await userClient.GetAsync($"/api/v1/users/{createdUser.Id}", cancellationToken);
        initialResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        await client.BlockAsync(createdUser.Id, cancellationToken);
        var user = await UserDatabaseTestHelper.FindAsync(
            Fixture,
            createdUser.Id,
            cancellationToken);

        user.ShouldNotBeNull();
        user.Status.ShouldBe("Blocked");
        using var response = await userClient.GetAsync($"/api/v1/users/{createdUser.Id}", cancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
