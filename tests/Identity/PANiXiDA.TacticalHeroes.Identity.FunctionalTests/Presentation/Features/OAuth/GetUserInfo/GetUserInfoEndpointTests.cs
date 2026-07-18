using System.Net.Http.Headers;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.GetUserInfo;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth.GetUserInfo;

public sealed class GetUserInfoEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "GET OAuth userinfo should return persisted user claims for an access token")]
    public async Task GetUserInfo_Should_ReturnCurrentUser_When_AccessTokenIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var createdUser = await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            Fixture,
            "userinfo@example.test",
            "userinfo-hero",
            "StrongPassword1!",
            cancellationToken);
        using var client = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var tokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            client,
            "userinfo@example.test",
            "StrongPassword1!",
            cancellationToken);
        using var request = new HttpRequestMessage(HttpMethod.Get, "/connect/userinfo");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        using var response = await client.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        var userInfo = await response.Content.ReadFromJsonAsync<GetUserInfoResponse>(
            JsonOptions,
            cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, responseBody);
        userInfo.ShouldNotBeNull();
        userInfo.Subject.ShouldBe(createdUser.Id.ToString());
        userInfo.Name.ShouldBe("userinfo-hero");
        userInfo.Email.ShouldBe("userinfo@example.test");
        userInfo.EmailVerified.ShouldBe(true);
    }
}
