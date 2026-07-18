using System.Net.Http.Headers;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.ChangePassword;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Auth.ChangePassword;

public sealed class ChangePasswordEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    private const string CurrentPassword = "StrongPassword1!";
    private const string NewPassword = "NewStrongPassword1!";

    [Fact(DisplayName = "POST auth change-password should persist the new password for the current user")]
    public async Task PostChangePassword_Should_ReplaceCurrentUserPassword_When_AccessTokenIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var createdUser = await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            Fixture,
            "change-password@example.test",
            "change-password-hero",
            CurrentPassword,
            cancellationToken);
        using var client = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var tokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            client,
            "change-password@example.test",
            CurrentPassword,
            cancellationToken);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/change-password")
        {
            Content = JsonContent.Create(
                new ChangePasswordRequest(CurrentPassword, NewPassword),
                options: JsonOptions)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        using var response = await client.SendAsync(request, cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        await using var scope = Fixture.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByIdAsync(createdUser.Id.ToString());

        user.ShouldNotBeNull();
        (await userManager.CheckPasswordAsync(user, CurrentPassword)).ShouldBeFalse();
        (await userManager.CheckPasswordAsync(user, NewPassword)).ShouldBeTrue();
    }
}
