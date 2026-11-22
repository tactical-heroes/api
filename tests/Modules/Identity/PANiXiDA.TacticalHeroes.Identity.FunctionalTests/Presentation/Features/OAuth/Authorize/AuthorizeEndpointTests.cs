using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Login;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth.Authorize;

[Collection(FunctionalTestCollectionDefinition.Name)]
public sealed class AuthorizeEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "GetAuthorize should issue authorization code when user has no administrative permissions")]
    public async Task GetAuthorize_Should_IssueAuthorizationCode_When_UserHasNoAdministrativePermissions()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            fixture: Fixture,
            email: "authorize@example.test",
            userName: "authorize-user",
            password: "StrongPassword1!",
            claims: null,
            cancellationToken: cancellationToken);
        using var client = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);

        var code = await OAuthAuthorizationRequestTestHelper.LoginAndGetAuthorizationCodeAsync(
            client,
            "authorize@example.test",
            "StrongPassword1!",
            cancellationToken);

        code.ShouldNotBeNullOrWhiteSpace();
    }

    [Theory(DisplayName = "GetAuthorize should reject session when user becomes unavailable")]
    [InlineData("Blocked", true, false)]
    [InlineData("Active", false, false)]
    [InlineData("Active", true, true)]
    public async Task GetAuthorize_Should_RejectSession_When_UserBecomesUnavailable(
        string status,
        bool isConfirmed,
        bool isDeleted)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var user = await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            fixture: Fixture,
            email: "authorize-unavailable@example.test",
            userName: "authorize-unavailable-user",
            password: "StrongPassword1!",
            claims: null,
            cancellationToken: cancellationToken);
        using var client = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var authorizePath = await OAuthAuthorizationRequestTestHelper.BuildAuthorizePathFromParAsync(
            client: client,
            scopes: OAuthAuthorizationRequestTestHelper.DefaultScopes,
            redirectUri: "https://localhost:5173/oauth/callback",
            cancellationToken: cancellationToken);
        using var loginResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest(
                "authorize-unavailable@example.test",
                "StrongPassword1!",
                $"https://localhost{authorizePath}"),
            TestJsonSerializerOptions.Web,
            cancellationToken);
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        await using var scope = Fixture.Services.CreateAsyncScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var applicationUser = (await manager.FindByIdAsync(user.Id.ToString())).ShouldNotBeNull();
        applicationUser.Status = status;
        applicationUser.EmailConfirmed = isConfirmed;
        var updateResult = isDeleted
            ? await manager.DeleteAsync(applicationUser)
            : await manager.UpdateAsync(applicationUser);
        updateResult.Succeeded.ShouldBeTrue();

        using var response = await client.GetAsync(authorizePath, cancellationToken);

        if (isDeleted)
        {
            response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
            response.Headers.Location.ShouldNotBeNull();
            response.Headers.Location.GetLeftPart(UriPartial.Path).ShouldBe("https://localhost:5173/login");
            response.Headers.Location.Query.ShouldContain(
                $"returnUrl={Uri.EscapeDataString($"https://localhost{authorizePath}")}");
            return;
        }

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        response.Headers.Location.ShouldBeNull();
    }

    [Theory(
        DisplayName = "GET OAuth authorize should redirect an anonymous user to the configured login page when user is anonymous")]
    [InlineData(OAuthAuthorizationRequestTestHelper.RedirectUri)]
    [InlineData("https://dev.tactical-heroes.panixida.ru/oauth/callback")]
    public async Task GetAuthorize_Should_RedirectToConfiguredLogin_When_UserIsAnonymous(
        string redirectUri)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var authorizePath = await OAuthAuthorizationRequestTestHelper.BuildAuthorizePathFromParAsync(
            client,
            OAuthAuthorizationRequestTestHelper.DefaultScopes,
            redirectUri,
            cancellationToken);

        using var response = await client.GetAsync(authorizePath, cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location.GetLeftPart(UriPartial.Path).ShouldBe("https://localhost:5173/login");
        response.Headers.Location.Query.ShouldContain(
            $"returnUrl={Uri.EscapeDataString($"https://localhost{authorizePath}")}");
    }
}
