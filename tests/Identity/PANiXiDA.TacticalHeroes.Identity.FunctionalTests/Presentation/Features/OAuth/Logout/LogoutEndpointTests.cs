using System.Data.Common;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;

using OpenIddict.Abstractions;
using OpenIddict.Server;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth.Logout;

public sealed class LogoutEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Theory(DisplayName = "OAuth logout should revoke only the selected grant when a valid identity hint is supplied")]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Logout_Should_RevokeOnlySelectedGrant_When_ValidIdentityHintIsSupplied(bool post, bool hintOnly)
    {
        var ct = TestContext.Current.CancellationToken;
        await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            Fixture, "logout-grant@example.test", "logout-grant", "StrongPassword1!", null, ct);
        await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            Fixture, "unrelated-logout@example.test", "unrelated-logout", "StrongPassword1!", null, ct);
        using var current = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        using var other = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        using var unrelated = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        using var anonymous = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var tokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            current, "logout-grant@example.test", "StrongPassword1!", ct);
        var otherTokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            other, "logout-grant@example.test", "StrongPassword1!", ct);
        var unrelatedTokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            unrelated, "unrelated-logout@example.test", "StrongPassword1!", ct);
        var browser = hintOnly ? anonymous : current;

        using var response = await LogoutAsync(browser, tokens.IdToken, post, ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Redirect, await response.Content.ReadAsStringAsync(ct));
        using var access = new HttpRequestMessage(HttpMethod.Get, "/connect/userinfo");
        access.Headers.Authorization = new("Bearer", tokens.AccessToken);
        using var userInfo = await current.SendAsync(access, ct);
        userInfo.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        using var refresh = await RefreshAsync(current, tokens.RefreshToken, ct);
        refresh.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        using var otherRefresh = await RefreshAsync(other, otherTokens.RefreshToken, ct);
        otherRefresh.StatusCode.ShouldBe(HttpStatusCode.OK);
        using var unrelatedRefresh = await RefreshAsync(unrelated, unrelatedTokens.RefreshToken, ct);
        unrelatedRefresh.StatusCode.ShouldBe(HttpStatusCode.OK);
        using var repeated = await LogoutAsync(browser, tokens.IdToken, post, ct);
        repeated.StatusCode.ShouldBeOneOf(HttpStatusCode.Redirect, HttpStatusCode.BadRequest);
    }

    [Theory(DisplayName = "OAuth logout should retain grants when identity hint is invalid or belongs to another user")]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Logout_Should_RetainGrants_When_HintIsInvalidOrBelongsToAnotherUser(bool post, bool foreign)
    {
        var ct = TestContext.Current.CancellationToken;
        await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            Fixture, "logout-owner@example.test", "logout-owner", "StrongPassword1!", null, ct);
        await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            Fixture, "logout-other@example.test", "logout-other", "StrongPassword1!", null, ct);
        using var owner = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        using var other = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var ownerTokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            owner, "logout-owner@example.test", "StrongPassword1!", ct);
        var otherTokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            other, "logout-other@example.test", "StrongPassword1!", ct);

        using var response = await LogoutAsync(other, foreign ? ownerTokens.IdToken : "invalid-token", post, ct);

        response.StatusCode.ShouldBe(foreign ? HttpStatusCode.Forbidden : HttpStatusCode.BadRequest);
        if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            cookies.ShouldNotContain(cookie => cookie.Contains(
                ".AspNetCore.Identity.Application=;",
                StringComparison.Ordinal));
        }
        using var ownerRefresh = await RefreshAsync(owner, ownerTokens.RefreshToken, ct);
        ownerRefresh.StatusCode.ShouldBe(HttpStatusCode.OK);
        using var otherRefresh = await RefreshAsync(other, otherTokens.RefreshToken, ct);
        otherRefresh.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact(
        DisplayName = "GET OAuth logout should clear the application cookie and redirect to the SPA when user is logged in")]
    public async Task GetLogout_Should_ClearAuthenticationSession_When_UserIsLoggedIn()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            fixture: Fixture,
            email: "logout@example.test",
            userName: "logout-hero",
            password: "StrongPassword1!",
            claims: null,
            cancellationToken: cancellationToken);
        using var client = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var tokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            client, "logout@example.test", "StrongPassword1!", cancellationToken);
        using var logoutResponse = await LogoutAsync(client, tokens.IdToken, false, cancellationToken);
        var logoutResponseBody = await logoutResponse.Content.ReadAsStringAsync(cancellationToken);

        logoutResponse.StatusCode.ShouldBe(HttpStatusCode.Redirect, logoutResponseBody);
        logoutResponse.Headers.Location.ShouldNotBeNull();
        logoutResponse.Headers.Location.GetLeftPart(UriPartial.Path)
            .ShouldBe("https://localhost:5173/oauth/logout-callback");
        OAuthAuthorizationRequestTestHelper.GetQueryParameter(
            logoutResponse.Headers.Location,
            OpenIddictConstants.Parameters.State)
            .ShouldBe("logout-state");
        logoutResponse.Headers.TryGetValues("Set-Cookie", out var cookies).ShouldBeTrue();
        cookies.ShouldContain(cookie =>
            cookie.Contains(".AspNetCore.Identity.Application=;", StringComparison.Ordinal));

        var nextAuthorizePath = await OAuthAuthorizationRequestTestHelper.BuildAuthorizePathFromParAsync(
            client: client,
            scopes: OAuthAuthorizationRequestTestHelper.DefaultScopes,
            redirectUri: "https://localhost:5173/oauth/callback",
            cancellationToken: cancellationToken);
        using var authorizeResponse = await client.GetAsync(nextAuthorizePath, cancellationToken);

        authorizeResponse.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        authorizeResponse.Headers.Location.ShouldNotBeNull();
        authorizeResponse.Headers.Location.GetLeftPart(UriPartial.Path).ShouldBe("https://localhost:5173/login");
    }

    [Theory(DisplayName = "OAuth logout should require a grant proof before revoking tokens or clearing the cookie")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Logout_Should_RejectMissingHint_When_UserHasAnActiveGrant(bool post)
    {
        var ct = TestContext.Current.CancellationToken;
        await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            Fixture, "logout-no-hint@example.test", "logout-no-hint", "StrongPassword1!", null, ct);
        using var client = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var tokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            client, "logout-no-hint@example.test", "StrongPassword1!", ct);

        using var response = await LogoutAsync(client, null, post, ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            cookies.ShouldNotContain(cookie => cookie.Contains(
                ".AspNetCore.Identity.Application=;", StringComparison.Ordinal));
        }
        using var refresh = await RefreshAsync(client, tokens.RefreshToken, ct);
        refresh.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Theory(DisplayName = "OAuth logout should reject cached and late tokens when another host refreshes the session")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Logout_Should_RejectCachedAndLateTokens_When_AnotherHostRefreshesSession(bool concurrent)
    {
        var ct = TestContext.Current.CancellationToken;
        await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            Fixture, "logout-cache@example.test", "logout-cache", "StrongPassword1!", null, ct);
        using var current = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        using var other = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var tokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            current, "logout-cache@example.test", "StrongPassword1!", ct);
        var otherTokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            other, "logout-cache@example.test", "StrongPassword1!", ct);
        var reached = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var resume = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var pause = false;
        await using var baseFactory = new FunctionalTestWebApplicationFactory();
        await using var factory = baseFactory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.AddOpenIddict().AddServer(options =>
                options.AddEventHandler<OpenIddictServerEvents.ProcessSignInContext>(builder => builder
                    .SetOrder(OpenIddictServerHandlers.GenerateAccessToken.Descriptor.Order - 500)
                    .UseInlineHandler(async context =>
                    {
                        if (pause && context.Request.IsRefreshTokenGrantType())
                        {
                            pause = false;
                            reached.TrySetResult();
                            await resume.Task.WaitAsync(context.CancellationToken);
                        }
                    })))));
        using var secondHost = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
        using var warm = await RefreshAsync(secondHost, tokens.RefreshToken, ct);
        warm.StatusCode.ShouldBe(HttpStatusCode.OK);
        using var warmBody = JsonDocument.Parse(await warm.Content.ReadAsStringAsync(ct));
        var accessToken = warmBody.RootElement.GetProperty("access_token").GetString()!;
        var refreshToken = warmBody.RootElement.GetProperty("refresh_token").GetString()!;
        pause = concurrent;
        Task<HttpResponseMessage>? pending = concurrent ? RefreshAsync(secondHost, refreshToken, ct) : null;

        try
        {
            if (concurrent)
            {
                await reached.Task.WaitAsync(TimeSpan.FromSeconds(30), ct);
            }
            using var logout = await LogoutAsync(current, tokens.IdToken, false, ct);
            logout.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        }
        finally
        {
            resume.TrySetResult();
        }
        if (pending is not null)
        {
            using var issued = await pending;
            issued.StatusCode.ShouldBe(HttpStatusCode.OK);
            using var body = JsonDocument.Parse(await issued.Content.ReadAsStringAsync(ct));
            accessToken = body.RootElement.GetProperty("access_token").GetString()!;
            refreshToken = body.RootElement.GetProperty("refresh_token").GetString()!;
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, "/connect/userinfo");
        request.Headers.Authorization = new("Bearer", accessToken);
        using var access = await secondHost.SendAsync(request, ct);
        access.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        using var refresh = await RefreshAsync(secondHost, refreshToken, ct);
        refresh.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        using var otherRefresh = await RefreshAsync(other, otherTokens.RefreshToken, ct);
        otherRefresh.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "OAuth logout should roll back revocation when cancellation occurs before commit")]
    public async Task Logout_Should_RollBackRevocation_When_CancellationOccursBeforeCommit()
    {
        var ct = TestContext.Current.CancellationToken;
        await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            Fixture, "logout-cancel@example.test", "logout-cancel", "StrongPassword1!", null, ct);
        using var current = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var tokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            current, "logout-cancel@example.test", "StrongPassword1!", ct);
        var authorizationId = Guid.Parse(new JsonWebToken(tokens.IdToken)
            .GetClaim(OpenIddictConstants.Claims.Private.AuthorizationId).Value);
        using var cancellation = new CancellationTokenSource();
        var interceptor = new CancelSessionRevocationInterceptor(authorizationId, cancellation);
        await using var baseFactory = new FunctionalTestWebApplicationFactory();
        await using var factory = baseFactory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.AddSingleton<IInterceptor>(interceptor)));
        using var client = new HttpClient(factory.Server.CreateHandler(context =>
            context.RequestAborted = cancellation.Token))
        {
            BaseAddress = new Uri("https://localhost")
        };

        using var response = await LogoutAsync(client, tokens.IdToken, true, ct);

        ((int)response.StatusCode).ShouldBe(499);
        response.Headers.Contains("Set-Cookie").ShouldBeFalse();
        interceptor.SavedInTransaction.ShouldBeTrue();
        using var refresh = await RefreshAsync(current, tokens.RefreshToken, ct);
        refresh.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    private static async Task<HttpResponseMessage> LogoutAsync(
        HttpClient client, string? identityToken, bool post, CancellationToken ct)
    {
        var parameters = new Dictionary<string, string>
        {
            [OpenIddictConstants.Parameters.ClientId] = OAuthAuthorizationRequestTestHelper.ClientId,
            [OpenIddictConstants.Parameters.State] = "logout-state",
            [OpenIddictConstants.Parameters.PostLogoutRedirectUri] = "https://localhost:5173/oauth/logout-callback"
        };
        if (identityToken is not null)
        {
            parameters[OpenIddictConstants.Parameters.IdTokenHint] = identityToken;
        }
        if (post)
        {
            using var content = new FormUrlEncodedContent(parameters);
            return await client.PostAsync("/connect/logout", content, ct);
        }

        var query = string.Join("&", parameters.Select(pair => $"{pair.Key}={Uri.EscapeDataString(pair.Value)}"));
        return await client.GetAsync("/connect/logout?" + query, ct);
    }

    private static async Task<HttpResponseMessage> RefreshAsync(
        HttpClient client, string refreshToken, CancellationToken ct)
    {
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            [OpenIddictConstants.Parameters.GrantType] = OpenIddictConstants.GrantTypes.RefreshToken,
            [OpenIddictConstants.Parameters.ClientId] = OAuthAuthorizationRequestTestHelper.ClientId,
            [OpenIddictConstants.Parameters.RefreshToken] = refreshToken
        });
        return await client.PostAsync("/connect/token", content, ct);
    }

    private sealed class CancelSessionRevocationInterceptor(Guid authorizationId, CancellationTokenSource cancellation)
        : DbCommandInterceptor
    {
        internal bool SavedInTransaction { get; private set; }

        public override async ValueTask<int> NonQueryExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            int result,
            CancellationToken cancellationToken)
        {
            if (command.CommandText.Contains("UPDATE", StringComparison.OrdinalIgnoreCase)
                && command.CommandText.Contains("open_iddict_authorizations", StringComparison.Ordinal)
                && command.Parameters.Cast<DbParameter>().Any(parameter => Equals(parameter.Value, authorizationId)))
            {
                SavedInTransaction = eventData.Context?.Database.CurrentTransaction is not null;
                await cancellation.CancelAsync();
            }

            return result;
        }
    }
}
