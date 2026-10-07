using System.Net.Http.Headers;
using System.Text.Json;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

using OpenIddict.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Events;
using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.ForgotPassword;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.ResetPassword;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Auth.ForgotPassword;

public sealed class ForgotPasswordEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    private const string CurrentPassword = "StrongPassword1!";
    private const string NewPassword = "NewStrongPassword1!";

    [Fact(DisplayName = "Password reset should replace the password and revoke existing tokens when issued token is valid")]
    public async Task PasswordReset_Should_ReplacePasswordAndRevokeTokens_When_IssuedTokenIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var createdUser = await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            Fixture,
            "forgot@example.test",
            "forgot-hero",
            CurrentPassword,
            cancellationToken);
        using var client = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var tokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            client,
            "forgot@example.test",
            CurrentPassword,
            cancellationToken);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        using var initialUserResponse = await client.GetAsync($"/api/v1/users/{createdUser.Id}", cancellationToken);
        initialUserResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        Fixture.EventBus.Clear();

        using var forgotResponse = await Client.PostAsJsonAsync(
            "/api/v1/auth/forgot-password",
            new ForgotPasswordRequest("forgot@example.test"),
            JsonOptions,
            cancellationToken);

        forgotResponse.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        var passwordReset = Fixture.EventBus.Single<PasswordResetRequested>();

        using var resetResponse = await Client.PostAsJsonAsync(
            "/api/v1/auth/reset-password",
            new ResetPasswordRequest(
                createdUser.Id,
                passwordReset.PasswordResetToken,
                NewPassword),
            JsonOptions,
            cancellationToken);

        resetResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        using var userResponse = await client.GetAsync($"/api/v1/users/{createdUser.Id}", cancellationToken);
        userResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        client.DefaultRequestHeaders.Authorization = null;
        using var refreshResponse = await client.PostAsync(
            "/connect/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                [OpenIddictConstants.Parameters.GrantType] = OpenIddictConstants.GrantTypes.RefreshToken,
                [OpenIddictConstants.Parameters.ClientId] = OAuthAuthorizationRequestTestHelper.ClientId,
                [OpenIddictConstants.Parameters.RefreshToken] = tokens.RefreshToken
            }),
            cancellationToken);
        refreshResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        using var refreshError = JsonDocument.Parse(await refreshResponse.Content.ReadAsStringAsync(cancellationToken));
        refreshError.RootElement.GetProperty(OpenIddictConstants.Parameters.Error)
            .GetString().ShouldBe(OpenIddictConstants.Errors.InvalidGrant);

        using var newClient = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var newTokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            newClient,
            "forgot@example.test",
            NewPassword,
            cancellationToken);
        newClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", newTokens.AccessToken);
        using var newUserResponse = await newClient.GetAsync($"/api/v1/users/{createdUser.Id}", cancellationToken);
        newUserResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        await using var scope = Fixture.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByIdAsync(createdUser.Id.ToString());

        user.ShouldNotBeNull();
        (await userManager.CheckPasswordAsync(user, CurrentPassword)).ShouldBeFalse();
        (await userManager.CheckPasswordAsync(user, NewPassword)).ShouldBeTrue();
    }
}
