using System.Net.Http.Headers;
using System.Text.Json;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;

using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.ChangePassword;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Auth.ChangePassword;

public sealed class ChangePasswordEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    private const string CurrentPassword = "StrongPassword1!";
    private const string NewPassword = "NewStrongPassword1!";

    [Fact(DisplayName = "Password change should preserve the current session and reject other tokens and cookies")]
    public async Task PostChangePassword_Should_RevokeOtherSessions_When_CurrentPasswordIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var user = await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            Fixture, "sessions@example.test", "sessions-hero", CurrentPassword, cancellationToken);
        var otherUser = await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            Fixture, "unaffected@example.test", "unaffected-hero", CurrentPassword, cancellationToken);
        using var current = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        using var other = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        using var unrelated = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var currentTokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            current, "sessions@example.test", CurrentPassword, cancellationToken);
        var otherTokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            other, "sessions@example.test", CurrentPassword, cancellationToken);
        var unrelatedTokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            unrelated, "unaffected@example.test", CurrentPassword, cancellationToken);
        var pendingCode = await OAuthAuthorizationRequestTestHelper.LoginAndGetAuthorizationCodeAsync(
            other, "sessions@example.test", CurrentPassword, cancellationToken);

        using var response = await ChangePasswordAsync(current, currentTokens.AccessToken,
            CurrentPassword, NewPassword, cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        response.Headers.Contains("Set-Cookie").ShouldBeTrue();
        await AssertAccessAsync(current, currentTokens.AccessToken, user.Id, HttpStatusCode.OK, cancellationToken);
        await AssertAccessAsync(other, otherTokens.AccessToken, user.Id, HttpStatusCode.Unauthorized, cancellationToken);
        await AssertAccessAsync(unrelated, unrelatedTokens.AccessToken, otherUser.Id, HttpStatusCode.OK, cancellationToken);
        using var refresh = await RefreshAsync(current, currentTokens.RefreshToken, cancellationToken);
        refresh.StatusCode.ShouldBe(HttpStatusCode.OK);
        using var tokens = JsonDocument.Parse(await refresh.Content.ReadAsStringAsync(cancellationToken));
        var accessToken = tokens.RootElement.GetProperty("access_token").GetString();
        var refreshToken = tokens.RootElement.GetProperty("refresh_token").GetString();
        accessToken.ShouldNotBeNullOrWhiteSpace();
        refreshToken.ShouldNotBeNullOrWhiteSpace();
        accessToken.ShouldNotBe(currentTokens.AccessToken);
        refreshToken.ShouldNotBe(currentTokens.RefreshToken);
        await AssertAccessAsync(current, accessToken, user.Id, HttpStatusCode.OK, cancellationToken);
        using var nextRefresh = await RefreshAsync(current, refreshToken, cancellationToken);
        nextRefresh.StatusCode.ShouldBe(HttpStatusCode.OK);
        using var rejectedRefresh = await RefreshAsync(other, otherTokens.RefreshToken, cancellationToken);
        await AssertInvalidGrantAsync(rejectedRefresh, cancellationToken);
        using var rejectedCode = await other.PostAsync("/connect/token", new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                [OpenIddictConstants.Parameters.GrantType] = OpenIddictConstants.GrantTypes.AuthorizationCode,
                [OpenIddictConstants.Parameters.ClientId] = OAuthAuthorizationRequestTestHelper.ClientId,
                [OpenIddictConstants.Parameters.Code] = pendingCode,
                [OpenIddictConstants.Parameters.CodeVerifier] = OAuthAuthorizationRequestTestHelper.CodeVerifier,
                [OpenIddictConstants.Parameters.RedirectUri] = OAuthAuthorizationRequestTestHelper.RedirectUri
            }), cancellationToken);
        await AssertInvalidGrantAsync(rejectedCode, cancellationToken);
        await AssertCookieAsync(current, OAuthAuthorizationRequestTestHelper.RedirectUri, cancellationToken);
        await AssertCookieAsync(other, "https://localhost:5173/login", cancellationToken);
        await AssertCookieAsync(unrelated, OAuthAuthorizationRequestTestHelper.RedirectUri, cancellationToken);
        await AssertPasswordAsync(user.Id, NewPassword, CurrentPassword);
    }

    [Theory(DisplayName = "Password change should preserve all sessions when password validation fails")]
    [InlineData("WrongPassword1!", NewPassword)]
    [InlineData(CurrentPassword, "weak")]
    public async Task PostChangePassword_Should_PreserveSessions_When_PasswordValidationFails(
        string currentPassword,
        string newPassword)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var user = await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            Fixture, "failed-change@example.test", "failed-change-hero", CurrentPassword, cancellationToken);
        using var current = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        using var other = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var currentTokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            current, "failed-change@example.test", CurrentPassword, cancellationToken);
        var otherTokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            other, "failed-change@example.test", CurrentPassword, cancellationToken);

        using var response = await ChangePasswordAsync(current, currentTokens.AccessToken,
            currentPassword, newPassword, cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        await AssertAccessAsync(current, currentTokens.AccessToken, user.Id, HttpStatusCode.OK, cancellationToken);
        await AssertAccessAsync(other, otherTokens.AccessToken, user.Id, HttpStatusCode.OK, cancellationToken);
        using var refresh = await RefreshAsync(other, otherTokens.RefreshToken, cancellationToken);
        refresh.StatusCode.ShouldBe(HttpStatusCode.OK);
        await AssertCookieAsync(current, OAuthAuthorizationRequestTestHelper.RedirectUri, cancellationToken);
        await AssertCookieAsync(other, OAuthAuthorizationRequestTestHelper.RedirectUri, cancellationToken);
        await AssertPasswordAsync(user.Id, CurrentPassword, NewPassword);
    }

    [Fact(DisplayName = "Password change should preserve a bearer-only session without creating a login cookie")]
    public async Task PostChangePassword_Should_PreserveRefresh_When_CookieIsAbsent()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            Fixture, "bearer-change@example.test", "bearer-change-hero", CurrentPassword, cancellationToken);
        using var browser = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var tokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            browser, "bearer-change@example.test", CurrentPassword, cancellationToken);
        using var client = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);

        using var response = await ChangePasswordAsync(client, tokens.AccessToken,
            CurrentPassword, NewPassword, cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        response.Headers.Contains("Set-Cookie").ShouldBeFalse();
        using var refresh = await RefreshAsync(client, tokens.RefreshToken, cancellationToken);
        refresh.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "Token exchange should keep the source session so password change can revoke its descendants")]
    public async Task PostChangePassword_Should_RevokeExchangedToken_When_AnotherSessionChangesPassword()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var user = await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            Fixture, "exchange-session@example.test", "exchange-session-hero", CurrentPassword, cancellationToken);
        using var current = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        using var other = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var currentTokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            current, "exchange-session@example.test", CurrentPassword, cancellationToken);
        var otherTokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            other, "exchange-session@example.test", CurrentPassword, cancellationToken);
        await using (var scope = Fixture.Services.CreateAsyncScope())
        {
            var applications = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
            var application = await applications.FindByClientIdAsync(
                OAuthAuthorizationRequestTestHelper.ClientId, cancellationToken);
            application.ShouldNotBeNull();
            var descriptor = new OpenIddictApplicationDescriptor();
            await applications.PopulateAsync(descriptor, application, cancellationToken);
            descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.TokenExchange);
            await applications.UpdateAsync(application, descriptor, cancellationToken);
        }
        using var exchange = await other.PostAsync("/connect/token", new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                [OpenIddictConstants.Parameters.GrantType] = OpenIddictConstants.GrantTypes.TokenExchange,
                [OpenIddictConstants.Parameters.ClientId] = OAuthAuthorizationRequestTestHelper.ClientId,
                [OpenIddictConstants.Parameters.SubjectToken] = otherTokens.AccessToken,
                [OpenIddictConstants.Parameters.SubjectTokenType] = OpenIddictConstants.TokenTypeIdentifiers.AccessToken,
                [OpenIddictConstants.Parameters.RequestedTokenType] = OpenIddictConstants.TokenTypeIdentifiers.AccessToken,
                [OpenIddictConstants.Parameters.Scope] = OpenIddictConstants.Scopes.Profile
            }), cancellationToken);
        var exchangeBody = await exchange.Content.ReadAsStringAsync(cancellationToken);
        exchange.StatusCode.ShouldBe(HttpStatusCode.OK, exchangeBody);
        using var exchanged = JsonDocument.Parse(exchangeBody);
        var exchangedAccess = exchanged.RootElement.GetProperty("access_token").GetString();
        exchangedAccess.ShouldNotBeNullOrWhiteSpace();
        await using (var scope = Fixture.Services.CreateAsyncScope())
        {
            var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictTokenManager>();
            var source = await manager.FindByReferenceIdAsync(otherTokens.AccessToken, cancellationToken);
            var descendant = await manager.FindByReferenceIdAsync(exchangedAccess, cancellationToken);
            source.ShouldNotBeNull();
            descendant.ShouldNotBeNull();
            var sourceId = await manager.GetAuthorizationIdAsync(source, cancellationToken);
            sourceId.ShouldNotBeNullOrWhiteSpace();
            (await manager.GetAuthorizationIdAsync(descendant, cancellationToken)).ShouldBe(sourceId);
        }
        await AssertAccessAsync(other, exchangedAccess, user.Id, HttpStatusCode.OK, cancellationToken);

        using var response = await ChangePasswordAsync(current, currentTokens.AccessToken,
            CurrentPassword, NewPassword, cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        await AssertAccessAsync(other, exchangedAccess, user.Id, HttpStatusCode.Unauthorized, cancellationToken);
    }

    [Fact(DisplayName = "Password change should roll back password and revocations when canceled after revoking another session")]
    public async Task PostChangePassword_Should_RollBackAndAllowRetry_When_RevocationIsCanceled()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var user = await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            Fixture, "cancel-change@example.test", "cancel-change-hero", CurrentPassword, cancellationToken);
        using var current = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        using var other = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var currentTokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            current, "cancel-change@example.test", CurrentPassword, cancellationToken);
        var otherTokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            other, "cancel-change@example.test", CurrentPassword, cancellationToken);
        using var requestCancellation = new CancellationTokenSource();
        var interceptor = new CancelAfterRevocationInterceptor(user.Id.ToString(), requestCancellation);
        await using var factory = new FunctionalTestWebApplicationFactory()
            .WithWebHostBuilder(builder => builder.ConfigureServices(services =>
                services.AddSingleton<IInterceptor>(interceptor)));
        using var client = new HttpClient(factory.Server.CreateHandler(context =>
            context.RequestAborted = requestCancellation.Token))
        {
            BaseAddress = new Uri("https://localhost")
        };

        using var response = await ChangePasswordAsync(client, currentTokens.AccessToken,
            CurrentPassword, NewPassword, cancellationToken);

        ((int)response.StatusCode).ShouldBe(499);
        interceptor.RevocationSavedInTransaction.ShouldBeTrue();
        await AssertPasswordAsync(user.Id, CurrentPassword, NewPassword);
        await AssertAccessAsync(current, currentTokens.AccessToken, user.Id, HttpStatusCode.OK, cancellationToken);
        await AssertAccessAsync(other, otherTokens.AccessToken, user.Id, HttpStatusCode.OK, cancellationToken);
        await AssertCookieAsync(other, OAuthAuthorizationRequestTestHelper.RedirectUri, cancellationToken);
        using var refresh = await RefreshAsync(other, otherTokens.RefreshToken, cancellationToken);
        refresh.StatusCode.ShouldBe(HttpStatusCode.OK);
        using var retry = await ChangePasswordAsync(current, currentTokens.AccessToken,
            CurrentPassword, NewPassword, cancellationToken);
        retry.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        await AssertPasswordAsync(user.Id, NewPassword, CurrentPassword);
        await AssertAccessAsync(other, otherTokens.AccessToken, user.Id, HttpStatusCode.Unauthorized, cancellationToken);
    }

    private sealed class CancelAfterRevocationInterceptor(
        string subject,
        CancellationTokenSource requestCancellation) : SaveChangesInterceptor
    {
        public bool RevocationSavedInTransaction { get; private set; }

        public override ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken)
        {
            var context = eventData.Context;
            if (context?.ChangeTracker.Entries<OpenIddictEntityFrameworkCoreAuthorization<Guid>>()
                .Any(entry => entry.Entity.Subject == subject &&
                    entry.Entity.Status == OpenIddictConstants.Statuses.Revoked) == true)
            {
                RevocationSavedInTransaction = context.Database.CurrentTransaction is not null;
                requestCancellation.Cancel();
            }

            return ValueTask.FromResult(result);
        }
    }

    [Fact(DisplayName = "Authorization code should be rejected when a cookie was validated before a concurrent password change")]
    public async Task PostToken_Should_RejectCode_When_OldCookieAuthorizationFinishesAfterPasswordChange()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            Fixture, "concurrent-change@example.test", "concurrent-change-hero", CurrentPassword, cancellationToken);
        var validated = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var resume = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var pauseAuthorization = false;
        await using var factory = new FunctionalTestWebApplicationFactory()
            .WithWebHostBuilder(builder => builder.ConfigureServices(services =>
                services.PostConfigure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme, options =>
                {
                    var validatePrincipal = options.Events.OnValidatePrincipal;
                    options.Events.OnValidatePrincipal = async context =>
                    {
                        await validatePrincipal(context);
                        if (pauseAuthorization && context.Request.Path == "/connect/authorize")
                        {
                            pauseAuthorization = false;
                            validated.SetResult();
                            await resume.Task.WaitAsync(context.HttpContext.RequestAborted);
                        }
                    };
                })));
        var clientOptions = new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        };
        using var current = factory.CreateClient(clientOptions);
        using var other = factory.CreateClient(clientOptions);
        var currentTokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            current, "concurrent-change@example.test", CurrentPassword, cancellationToken);
        await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            other, "concurrent-change@example.test", CurrentPassword, cancellationToken);
        var authorizePath = await OAuthAuthorizationRequestTestHelper.BuildAuthorizePathFromParAsync(
            other, OAuthAuthorizationRequestTestHelper.DefaultScopes, cancellationToken);
        pauseAuthorization = true;
        var pendingAuthorization = other.GetAsync(authorizePath, cancellationToken);
        try
        {
            await validated.Task.WaitAsync(cancellationToken);
            using var changed = await ChangePasswordAsync(current, currentTokens.AccessToken,
                CurrentPassword, NewPassword, cancellationToken);
            changed.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        }
        finally
        {
            resume.TrySetResult();
        }
        using var authorizeResponse = await pendingAuthorization;
        authorizeResponse.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        authorizeResponse.Headers.Location.ShouldNotBeNull();
        var code = OAuthAuthorizationRequestTestHelper.GetQueryParameter(
            authorizeResponse.Headers.Location, OpenIddictConstants.Parameters.Code);

        using var response = await other.PostAsync("/connect/token", new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                [OpenIddictConstants.Parameters.GrantType] = OpenIddictConstants.GrantTypes.AuthorizationCode,
                [OpenIddictConstants.Parameters.ClientId] = OAuthAuthorizationRequestTestHelper.ClientId,
                [OpenIddictConstants.Parameters.Code] = code,
                [OpenIddictConstants.Parameters.CodeVerifier] = OAuthAuthorizationRequestTestHelper.CodeVerifier,
                [OpenIddictConstants.Parameters.RedirectUri] = OAuthAuthorizationRequestTestHelper.RedirectUri
            }), cancellationToken);

        await AssertInvalidGrantAsync(response, cancellationToken);
    }

    private static async Task<HttpResponseMessage> ChangePasswordAsync(
        HttpClient client,
        string accessToken,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/change-password")
        {
            Content = JsonContent.Create(new ChangePasswordRequest(currentPassword, newPassword), options: JsonOptions)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return await client.SendAsync(request, cancellationToken);
    }

    private static async Task AssertAccessAsync(
        HttpClient client,
        string accessToken,
        Guid userId,
        HttpStatusCode statusCode,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/users/{userId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await client.SendAsync(request, cancellationToken);
        response.StatusCode.ShouldBe(statusCode);
    }

    private static Task<HttpResponseMessage> RefreshAsync(
        HttpClient client,
        string refreshToken,
        CancellationToken cancellationToken)
    {
        return client.PostAsync("/connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            [OpenIddictConstants.Parameters.GrantType] = OpenIddictConstants.GrantTypes.RefreshToken,
            [OpenIddictConstants.Parameters.ClientId] = OAuthAuthorizationRequestTestHelper.ClientId,
            [OpenIddictConstants.Parameters.RefreshToken] = refreshToken
        }), cancellationToken);
    }

    private static async Task AssertInvalidGrantAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        using var error = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        error.RootElement.GetProperty("error").GetString().ShouldBe(OpenIddictConstants.Errors.InvalidGrant);
    }

    private static async Task AssertCookieAsync(
        HttpClient client,
        string redirectUri,
        CancellationToken cancellationToken)
    {
        var path = await OAuthAuthorizationRequestTestHelper.BuildAuthorizePathFromParAsync(
            client, OAuthAuthorizationRequestTestHelper.DefaultScopes, cancellationToken);
        using var response = await client.GetAsync(path, cancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location.GetLeftPart(UriPartial.Path).ShouldBe(redirectUri);
    }

    private async Task AssertPasswordAsync(Guid userId, string acceptedPassword, string rejectedPassword)
    {
        await using var scope = Fixture.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByIdAsync(userId.ToString());
        user.ShouldNotBeNull();
        (await userManager.CheckPasswordAsync(user, acceptedPassword)).ShouldBeTrue();
        (await userManager.CheckPasswordAsync(user, rejectedPassword)).ShouldBeFalse();
    }
}
