using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers.Events;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.IdentityUsers.Confirm;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.IdentityUsers.Me;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.IdentityUsers.PasswordReset;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.IdentityUsers.Register;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.IdentityUsers;

public sealed class IdentityUserAuthenticationFlowTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    private const string Email = "hero@example.com";
    private const string Password = "StrongPassword1";
    private const string NewPassword = "StrongerPassword2";

    [Fact(DisplayName = "Registered and confirmed user should login refresh and stay authorized after restart")]
    public async Task RegisteredAndConfirmedUser_Should_Login_Refresh_And_StayAuthorized_AfterRestart()
    {
        var userId = await RegisterAsync(Email, Password);

        var loginBeforeConfirmationResponse = await RequestPasswordTokenAsync(
            Email,
            Password);

        loginBeforeConfirmationResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var confirmationEvent = Fixture.EventBus.Single<AccountConfirmationRequested>();
        confirmationEvent.UserId.ShouldBe(userId);
        confirmationEvent.Email.ShouldBe(Email);

        var confirmationResponse = await Client.PostAsJsonAsync(
            "/api/v1/identity/auth/confirm",
            new ConfirmRegistrationRequest(userId, confirmationEvent.ConfirmationToken),
            TestContext.Current.CancellationToken);

        confirmationResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var registeredEvent = Fixture.EventBus.Single<IdentityUserRegistered>();
        registeredEvent.UserId.ShouldBe(userId);
        registeredEvent.Email.ShouldBe(Email);

        var token = await ReadTokenAsync(await RequestPasswordTokenAsync(Email, Password));

        var currentUser = await GetCurrentUserAsync(token.AccessToken);
        currentUser.UserId.ShouldBe(userId);
        currentUser.Email.ShouldBe(Email);

        Fixture.RestartApplication();

        var refreshedToken = await ReadTokenAsync(await RequestRefreshTokenAsync(token.RefreshToken));
        var currentUserAfterRestart = await GetCurrentUserAsync(refreshedToken.AccessToken);

        currentUserAfterRestart.UserId.ShouldBe(userId);
        currentUserAfterRestart.Email.ShouldBe(Email);
    }

    [Fact(DisplayName = "Confirmed user should reset password through password reset event token")]
    public async Task ConfirmedUser_Should_ResetPassword_ThroughPasswordResetEventToken()
    {
        var userId = await RegisterAndConfirmAsync(Email, Password);

        Fixture.EventBus.Clear();

        var requestResetResponse = await Client.PostAsJsonAsync(
            "/api/v1/identity/auth/password-reset/request",
            new RequestPasswordResetRequest(Email),
            TestContext.Current.CancellationToken);

        requestResetResponse.StatusCode.ShouldBe(HttpStatusCode.Accepted);

        var passwordResetEvent = Fixture.EventBus.Single<PasswordResetRequested>();
        passwordResetEvent.UserId.ShouldBe(userId);
        passwordResetEvent.Email.ShouldBe(Email);

        var resetPasswordResponse = await Client.PostAsJsonAsync(
            "/api/v1/identity/auth/password-reset/confirm",
            new ResetPasswordRequest(userId, passwordResetEvent.PasswordResetToken, NewPassword),
            TestContext.Current.CancellationToken);

        resetPasswordResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var oldPasswordResponse = await RequestPasswordTokenAsync(Email, Password);
        oldPasswordResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var newPasswordToken = await ReadTokenAsync(
            await RequestPasswordTokenAsync(Email, NewPassword));

        newPasswordToken.AccessToken.ShouldNotBeNullOrWhiteSpace();
        newPasswordToken.RefreshToken.ShouldNotBeNullOrWhiteSpace();
    }

    private async Task<Guid> RegisterAndConfirmAsync(
        string email,
        string password)
    {
        var userId = await RegisterAsync(email, password);
        var confirmationEvent = Fixture.EventBus.Single<AccountConfirmationRequested>();

        var confirmationResponse = await Client.PostAsJsonAsync(
            "/api/v1/identity/auth/confirm",
            new ConfirmRegistrationRequest(userId, confirmationEvent.ConfirmationToken),
            TestContext.Current.CancellationToken);

        confirmationResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        return userId;
    }

    private async Task<Guid> RegisterAsync(
        string email,
        string password)
    {
        var registerResponse = await Client.PostAsJsonAsync(
            "/api/v1/identity/auth/register",
            new RegisterUserRequest(email, password),
            TestContext.Current.CancellationToken);

        registerResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var response = await registerResponse.Content.ReadFromJsonAsync<RegisterUserResponse>(
            JsonOptions,
            TestContext.Current.CancellationToken);

        response.ShouldNotBeNull();

        return response.UserId;
    }

    private Task<HttpResponseMessage> RequestPasswordTokenAsync(
        string email,
        string password)
    {
        return Client.PostAsync(
            "/connect/token",
            new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["grant_type"] = "password",
                    ["username"] = email,
                    ["password"] = password,
                    ["scope"] = "email profile roles offline_access"
                }),
            TestContext.Current.CancellationToken);
    }

    private Task<HttpResponseMessage> RequestRefreshTokenAsync(string refreshToken)
    {
        return Client.PostAsync(
            "/connect/token",
            new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["grant_type"] = "refresh_token",
                    ["refresh_token"] = refreshToken
                }),
            TestContext.Current.CancellationToken);
    }

    private async Task<OAuthTokenResponse> ReadTokenAsync(HttpResponseMessage response)
    {
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var token = await response.Content.ReadFromJsonAsync<OAuthTokenResponse>(
            JsonOptions,
            TestContext.Current.CancellationToken);

        token.ShouldNotBeNull();
        token.AccessToken.ShouldNotBeNullOrWhiteSpace();
        token.RefreshToken.ShouldNotBeNullOrWhiteSpace();

        return token;
    }

    private async Task<CurrentUserResponse> GetCurrentUserAsync(string accessToken)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await Client.GetAsync(
            "/api/v1/identity/auth/me",
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var currentUser = await response.Content.ReadFromJsonAsync<CurrentUserResponse>(
            JsonOptions,
            TestContext.Current.CancellationToken);

        currentUser.ShouldNotBeNull();

        return currentUser;
    }

    private sealed record OAuthTokenResponse(
        [property: JsonPropertyName("access_token")]
        string AccessToken,
        [property: JsonPropertyName("refresh_token")]
        string RefreshToken,
        [property: JsonPropertyName("token_type")]
        string TokenType);
}
