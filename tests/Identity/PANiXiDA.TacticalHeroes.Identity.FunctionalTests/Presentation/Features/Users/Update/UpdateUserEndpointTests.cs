using System.Net.Http.Headers;
using System.Text.Json;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;
using OpenIddict.Server;

using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;
using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Roles;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.ResetPassword;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Create;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Create;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Update;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Users.Update;

public sealed class UpdateUserEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    private const string Password = "StrongPassword1!";
    private const string Email = "role-session@example.test";

    [Theory(DisplayName = "PUT user should not restore a cookie when password reset races claims read")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PutUser_Should_NotRestoreCookie_When_PasswordResetRacesClaimsRead(bool readBeforePause)
    {
        var ct = TestContext.Current.CancellationToken;
        var (user, _) = await CreateUserAsync(assigned: false, claims: null, ct: ct);
        var reached = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var resume = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var pause = false;
        await using var baseFactory = new FunctionalTestWebApplicationFactory();
        await using var factory = baseFactory.WithAuthenticatedUserReadPause(
            reached, resume, readBeforePause, () => pause);
        using var browser = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
        var tokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(browser, Email, Password, ct);
        pause = true;

        var pending = UpdateAsync(
            client: browser,
            userId: user.Id,
            roles: [],
            accessToken: tokens.AccessToken,
            status: "Active",
            isConfirmed: true,
            claims: [new("permission", "identity.users.update"), new("permission", "identity.roles.read")],
            ct: ct);
        try
        {
            await reached.Task.WaitAsync(TimeSpan.FromSeconds(30), ct);
            await using var scope = Fixture.Services.CreateAsyncScope();
            var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var stored = await manager.FindByIdAsync(user.Id.ToString());
            stored.ShouldNotBeNull();
            var token = await manager.GeneratePasswordResetTokenAsync(stored);
            using var reset = await Client.PostAsJsonAsync(
                "/api/v1/auth/reset-password", new ResetPasswordRequest(user.Id, token, "OtherStrongPassword2!"), ct);
            reset.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        }
        finally
        {
            resume.TrySetResult();
        }
        using var response = await pending;

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        await AssertCookieAsync(browser, false, ct);
        using var refresh = await RefreshAsync(browser, tokens.RefreshToken, ct);
        refresh.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "PUT user should update persisted user state when request is valid")]
    public async Task PutUser_Should_UpdatePostgreSql_When_RequestIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new UserApiTestClient(Fixture);
        var createdUser = await client.CreateAsync(request: null, cancellationToken: cancellationToken);

        await client.UpdateAsync(
            createdUser.Id,
            new UpdateUserRequest(
                "updated@example.com",
                "updated-hero",
                false,
                [new Claim("permission", "compendium.heroes.update")],
                "Blocked",
                RoleIds: []),
            cancellationToken);
        var user = await UserDatabaseTestHelper.FindAsync(
            Fixture,
            createdUser.Id,
            cancellationToken);

        user.ShouldNotBeNull();
        user.Email.ShouldBe("updated@example.com");
        user.UserName.ShouldBe("updated-hero");
        user.EmailConfirmed.ShouldBeFalse();
        user.Status.ShouldBe("Blocked");
        user.Claims.ShouldHaveSingleItem().ClaimValue.ShouldBe("compendium.heroes.update");
    }

    [Theory(DisplayName = "User edits should replace or clear roles when role selections vary")]
    [InlineData("replace")]
    [InlineData("clear")]
    public async Task PutUser_Should_ApplyRoleSelection_When_RoleSelectionsVary(string mode)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var roles = (await new RoleApiTestClient(Fixture).GetListAsync(cancellationToken)).Items;
        var developerId = roles.Single(role => role.Name == "Developer").Id;
        var moderatorId = roles.Single(role => role.Name == "Moderator").Id;
        var client = new UserApiTestClient(Fixture);
        var created = await client.CreateAsync(
            UserApiTestClient.CreateDefaultRequest() with { RoleIds = [developerId] },
            cancellationToken);
        IReadOnlyCollection<Guid> selected = mode == "clear" ? [] : [moderatorId];
        var payload = JsonSerializer.SerializeToNode(
            new UpdateUserRequest(
                Email: "edited@example.com",
                UserName: "edited",
                IsConfirmed: true,
                Claims: [],
                Status: "Active",
                RoleIds: selected),
            JsonOptions)!
            .AsObject();
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/users/{created.Id}")
        {
            Content = JsonContent.Create(payload, options: JsonOptions)
        };

        using var response = await OAuthServiceAccessTokenTestHelper.SendAsync(Fixture, request, cancellationToken);
        var details = await client.GetDetailsAsync(created.Id, cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        details.Email.ShouldBe("edited@example.com");
        details.RoleIds.ShouldBe(selected, ignoreOrder: true);
    }

    [Theory(DisplayName = "PUT user should reject invalid role selections when role selection is invalid")]
    [InlineData("omitted")]
    [InlineData("null")]
    [InlineData("missing")]
    [InlineData("empty")]
    [InlineData("duplicate")]
    public async Task PutUser_Should_ReturnBadRequest_When_RoleSelectionIsInvalid(string invalid)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new UserApiTestClient(Fixture);
        var roles = (await new RoleApiTestClient(Fixture).GetListAsync(cancellationToken)).Items;
        var developerId = roles.Single(role => role.Name == "Developer").Id;
        Guid[] roleIds = invalid switch
        {
            "empty" => [Guid.Empty],
            "duplicate" => [developerId, developerId],
            _ => [developerId, Guid.CreateVersion7()]
        };
        var user = await client.CreateAsync(
            UserApiTestClient.CreateDefaultRequest() with { RoleIds = [developerId] },
            cancellationToken);
        var payload = JsonSerializer.SerializeToNode(
            new UpdateUserRequest(
                Email: "rejected@example.com",
                UserName: "hero",
                IsConfirmed: true,
                Claims: [],
                Status: "Active",
                RoleIds: roleIds),
            JsonOptions)!
            .AsObject();
        if (invalid == "omitted")
        {
            payload.Remove("roleIds");
        }
        else if (invalid == "null")
        {
            payload["roleIds"] = null;
        }
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/users/{user.Id}")
        {
            Content = JsonContent.Create(payload, options: JsonOptions)
        };

        using var response = await OAuthServiceAccessTokenTestHelper.SendAsync(Fixture, request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest, body);
        var details = await client.GetDetailsAsync(user.Id, cancellationToken);
        details.Email.ShouldBe("hero@example.com");
        details.RoleIds.ShouldBe([developerId]);
    }

    [Theory(DisplayName = "PUT user should reject unknown managed claims when managed claim is unknown")]
    [InlineData("permission")]
    [InlineData("section")]
    [InlineData("custom")]
    public async Task PutUser_Should_ReturnBadRequest_When_ManagedClaimIsUnknown(string type)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new UserApiTestClient(Fixture);
        var existing = await client.CreateAsync(request: null, cancellationToken: cancellationToken);
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/users/{existing.Id}")
        {
            Content = JsonContent.Create(
                new UpdateUserRequest(
                    Email: "hero@example.com",
                    UserName: "hero",
                    IsConfirmed: true,
                    Claims: [new Claim(type, "unknown")],
                    Status: "Active",
                    RoleIds: []),
                options: JsonOptions)
        };

        using var response = await OAuthServiceAccessTokenTestHelper.SendAsync(Fixture, request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest, body);
        using var problem = JsonDocument.Parse(body);
        problem.RootElement.GetProperty("errors")
            .GetProperty("claims[0].Value")
            .EnumerateArray()
            .Select(error => error.GetString())
            .ShouldBe(["Claim value is not present in its catalog."]);
        var details = await client.GetDetailsAsync(existing.Id, cancellationToken);
        details.Claims.ShouldBeEmpty();
    }

    [Theory(DisplayName = "PUT user should return domain validation errors when status is invalid")]
    [InlineData("", "User status is required.")]
    [InlineData("   ", "User status is required.")]
    [InlineData("Deleted", "User status 'Deleted' is invalid.")]
    public async Task PutUser_Should_ReturnValidationProblem_When_StatusIsInvalid(
        string status,
        string expectedMessage)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new UserApiTestClient(Fixture);
        var user = await client.CreateAsync(request: null, cancellationToken: cancellationToken);
        var payload = JsonSerializer.SerializeToNode(
            UserApiTestClient.CreateDefaultRequest() with { Status = status }, JsonOptions)!
            .AsObject();
        payload.Remove("password");
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/users/{user.Id}")
        {
            Content = JsonContent.Create(payload, options: JsonOptions)
        };

        using var response = await OAuthServiceAccessTokenTestHelper.SendAsync(Fixture, request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest, responseBody);
        using var problem = JsonDocument.Parse(responseBody);
        var errors = problem.RootElement.GetProperty("errors")
            .GetProperty("status")
            .EnumerateArray()
            .Select(error => error.GetString())
            .ToArray();
        errors.ShouldBe([expectedMessage]);
    }

    [Theory(DisplayName = "PUT user should return bad request when status is missing or null")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PutUser_Should_ReturnBadRequest_When_StatusIsMissingOrNull(bool omitStatus)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new UserApiTestClient(Fixture);
        var user = await client.CreateAsync(request: null, cancellationToken: cancellationToken);
        var payload = JsonSerializer.SerializeToNode(UserApiTestClient.CreateDefaultRequest(), JsonOptions)!.AsObject();
        if (omitStatus)
        {
            payload.Remove("status");
        }
        else
        {
            payload["status"] = null;
        }
        payload.Remove("password");
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/users/{user.Id}")
        {
            Content = JsonContent.Create(payload, options: JsonOptions)
        };

        using var response = await OAuthServiceAccessTokenTestHelper.SendAsync(Fixture, request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest, responseBody);
    }

    [Theory(DisplayName = "PUT user should revoke sessions and refresh current permissions when authorization changes")]
    [InlineData(true, true, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, false)]
    [InlineData(true, true, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(false, false, true)]
    public async Task PutUser_Should_InvalidateSessions_When_AuthorizationChanges(
        bool self, bool removePermission, bool changeClaims)
    {
        var ct = TestContext.Current.CancellationToken;
        var (user, roleId) = await CreateUserAsync(
            assigned: !changeClaims && removePermission,
            claims: changeClaims && removePermission
                ? [new("permission", "identity.users.update"), new("permission", "identity.roles.read")]
                : null,
            ct: ct);
        await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            fixture: Fixture,
            email: "unrelated@example.test",
            userName: "unrelated",
            password: Password,
            claims: [new("permission", "identity.users.update")],
            cancellationToken: ct);
        using var current = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        using var other = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        using var unrelated = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var tokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(current, Email, Password, ct);
        var otherTokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(other, Email, Password, ct);
        var unrelatedTokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            unrelated, "unrelated@example.test", Password, ct);

        using var response = await UpdateAsync(
            client: current,
            userId: user.Id,
            roles: changeClaims || removePermission ? [] : [roleId],
            accessToken: self ? tokens.AccessToken : unrelatedTokens.AccessToken,
            status: "Active",
            isConfirmed: true,
            claims: changeClaims && !removePermission
                ? [new("permission", "identity.users.update"), new("permission", "identity.roles.read")]
                : null,
            ct: ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent, await response.Content.ReadAsStringAsync(ct));
        response.Headers.Contains("X-Session-Refresh-Required").ShouldBe(self);
        if (self)
        {
            response.Headers.GetValues("X-Session-Refresh-Required").ShouldBe(["true"]);
        }
        await AssertAccessAsync(
            client: current,
            token: tokens.AccessToken,
            status: HttpStatusCode.Unauthorized,
            path: "/connect/userinfo",
            ct: ct);
        await AssertAccessAsync(
            client: other,
            token: otherTokens.AccessToken,
            status: HttpStatusCode.Unauthorized,
            path: "/connect/userinfo",
            ct: ct);
        await AssertAccessAsync(
            client: unrelated,
            token: unrelatedTokens.AccessToken,
            status: HttpStatusCode.OK,
            path: "/connect/userinfo",
            ct: ct);
        await AssertCookieAsync(other, false, ct);
        using var otherRefresh = await RefreshAsync(other, otherTokens.RefreshToken, ct);
        otherRefresh.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        using var refresh = await RefreshAsync(current, tokens.RefreshToken, ct);
        refresh.StatusCode.ShouldBe(self ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
        await AssertCookieAsync(current, self, ct);
        if (self)
        {
            response.Headers.Contains("Set-Cookie").ShouldBeTrue();
            using var document = JsonDocument.Parse(await refresh.Content.ReadAsStringAsync(ct));
            var access = document.RootElement.GetProperty("access_token").GetString()!;
            await AssertAccessAsync(
                client: current,
                token: access,
                status: HttpStatusCode.OK,
                path: "/connect/userinfo",
                ct: ct);
            await AssertAccessAsync(
                current,
                access,
                removePermission ? HttpStatusCode.Forbidden : HttpStatusCode.OK,
                "/api/v1/roles",
                ct);
            using var nextRefresh = await RefreshAsync(
                current, document.RootElement.GetProperty("refresh_token").GetString()!, ct);
            nextRefresh.StatusCode.ShouldBe(HttpStatusCode.OK);
        }
        else
        {
            var newTokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
                current, Email, Password, ct);
            await AssertAccessAsync(
                current,
                newTokens.AccessToken,
                removePermission ? HttpStatusCode.Forbidden : HttpStatusCode.OK,
                "/api/v1/roles",
                ct);
        }
    }

    [Theory(DisplayName = "PUT user should revoke all sessions when confirmation is removed")]
    [InlineData(true, "none")]
    [InlineData(false, "none")]
    [InlineData(true, "roles")]
    [InlineData(false, "roles")]
    [InlineData(true, "claims")]
    [InlineData(false, "claims")]
    public async Task PutUser_Should_RevokeAllSessions_When_ConfirmationIsRemoved(bool self, string change)
    {
        var ct = TestContext.Current.CancellationToken;
        var (user, roleId) = await CreateUserAsync(assigned: false, claims: null, ct: ct);
        await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            fixture: Fixture,
            email: "unrelated@example.test",
            userName: "unrelated",
            password: Password,
            claims: [new("permission", "identity.users.update")],
            cancellationToken: ct);
        using var current = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        using var other = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        using var unrelated = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var tokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(current, Email, Password, ct);
        var otherTokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(other, Email, Password, ct);
        var unrelatedTokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            unrelated, "unrelated@example.test", Password, ct);
        var before = await UserDatabaseTestHelper.FindAsync(Fixture, user.Id, ct);

        using var response = await UpdateAsync(
            client: self ? current : unrelated,
            userId: user.Id,
            roles: change == "roles" ? [roleId] : [],
            accessToken: self ? tokens.AccessToken : unrelatedTokens.AccessToken,
            status: "Active",
            isConfirmed: false,
            claims: change == "claims"
                ? [new("permission", "identity.users.update"), new("permission", "identity.roles.read")]
                : null,
            ct: ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent, await response.Content.ReadAsStringAsync(ct));
        response.Headers.Contains("X-Session-Refresh-Required").ShouldBeFalse();
        var after = await UserDatabaseTestHelper.FindAsync(Fixture, user.Id, ct);
        after!.EmailConfirmed.ShouldBeFalse();
        after.SecurityStamp.ShouldNotBe(before!.SecurityStamp);
        await AssertAccessAsync(
            client: current,
            token: tokens.AccessToken,
            status: HttpStatusCode.Unauthorized,
            path: "/connect/userinfo",
            ct: ct);
        await AssertAccessAsync(
            client: other,
            token: otherTokens.AccessToken,
            status: HttpStatusCode.Unauthorized,
            path: "/connect/userinfo",
            ct: ct);
        await AssertAccessAsync(
            client: unrelated,
            token: unrelatedTokens.AccessToken,
            status: HttpStatusCode.OK,
            path: "/connect/userinfo",
            ct: ct);
        await AssertCookieAsync(current, false, ct);
        await AssertCookieAsync(other, false, ct);
        await AssertCookieAsync(unrelated, true, ct);
        using var refresh = await RefreshAsync(current, tokens.RefreshToken, ct);
        refresh.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        using var otherRefresh = await RefreshAsync(other, otherTokens.RefreshToken, ct);
        otherRefresh.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        using var unrelatedRefresh = await RefreshAsync(unrelated, unrelatedTokens.RefreshToken, ct);
        unrelatedRefresh.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Theory(DisplayName = "PUT user should preserve sessions when update does not change authorization")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PutUser_Should_PreserveSessions_When_UpdateDoesNotChangeAuthorization(bool invalid)
    {
        var ct = TestContext.Current.CancellationToken;
        var (user, roleId) = await CreateUserAsync(
            true,
            [new("permission", "identity.roles.read"), new("permission", "identity.users.update")],
            ct);
        using var current = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var tokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(current, Email, Password, ct);
        var before = await UserDatabaseTestHelper.FindAsync(Fixture, user.Id, ct);

        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/users/{user.Id}")
        {
            Content = JsonContent.Create(new UpdateUserRequest(
                Email: Email,
                UserName: "updated-session",
                IsConfirmed: true,
                Claims:
                [
                    new("permission", "identity.users.update"),
                    new("permission", "identity.roles.read"),
                    new("permission", "identity.users.update")
                ],
                Status: "Active",
                RoleIds: invalid ? [Guid.CreateVersion7()] : [roleId]))
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        using var response = await current.SendAsync(request, ct);

        response.StatusCode.ShouldBe(invalid ? HttpStatusCode.BadRequest : HttpStatusCode.NoContent);
        response.Headers.Contains("X-Session-Refresh-Required").ShouldBeFalse();
        if (!invalid)
        {
            var options = Fixture.Services.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
                .Get(IdentityConstants.ApplicationScheme);
            var cookie = response.Headers.GetValues("Set-Cookie")
                .Select(value => SetCookieHeaderValue.Parse(value))
                .Single(header => header.Name == options.Cookie.Name);
            var ticket = options.TicketDataFormat.Unprotect(cookie.Value.ToString());
            ticket.ShouldNotBeNull();
            ticket.Principal.GetClaim(OpenIddictConstants.Claims.Name).ShouldBe("updated-session");
        }
        var after = await UserDatabaseTestHelper.FindAsync(Fixture, user.Id, ct);
        after!.SecurityStamp.ShouldBe(before!.SecurityStamp);
        await AssertAccessAsync(
            client: current,
            token: tokens.AccessToken,
            status: HttpStatusCode.OK,
            path: "/connect/userinfo",
            ct: ct);
        await AssertCookieAsync(current, true, ct);
        using var refresh = await RefreshAsync(current, tokens.RefreshToken, ct);
        refresh.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "PUT user should preserve refresh without renewing cookies when current cookie is absent")]
    public async Task PutUser_Should_PreserveRefresh_When_CurrentCookieIsAbsent()
    {
        var ct = TestContext.Current.CancellationToken;
        var (user, roleId) = await CreateUserAsync(assigned: false, claims: null, ct: ct);
        using var browser = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var tokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(browser, Email, Password, ct);
        using var client = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);

        using var response = await UpdateAsync(
            client: client,
            userId: user.Id,
            roles: [roleId],
            accessToken: tokens.AccessToken,
            status: "Active",
            isConfirmed: true,
            claims: null,
            ct: ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        response.Headers.Contains("Set-Cookie").ShouldBeFalse();
        response.Headers.GetValues("X-Session-Refresh-Required").ShouldBe(["true"]);
        await AssertCookieAsync(browser, false, ct);
        using var refresh = await RefreshAsync(client, tokens.RefreshToken, ct);
        refresh.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Theory(DisplayName = "PUT user should revoke current session when user becomes unavailable")]
    [InlineData("Blocked", true)]
    [InlineData("Active", false)]
    public async Task PutUser_Should_RevokeCurrentSession_When_UserBecomesUnavailable(string status, bool isConfirmed)
    {
        var ct = TestContext.Current.CancellationToken;
        var (user, roleId) = await CreateUserAsync(assigned: false, claims: null, ct: ct);
        using var current = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var tokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(current, Email, Password, ct);

        using var response = await UpdateAsync(
            client: current,
            userId: user.Id,
            roles: [roleId],
            accessToken: tokens.AccessToken,
            status: status,
            isConfirmed: isConfirmed,
            claims: null,
            ct: ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent, await response.Content.ReadAsStringAsync(ct));
        response.Headers.Contains("X-Session-Refresh-Required").ShouldBeFalse();
        await AssertAccessAsync(
            client: current,
            token: tokens.AccessToken,
            status: HttpStatusCode.Unauthorized,
            path: "/connect/userinfo",
            ct: ct);
        await AssertCookieAsync(current, false, ct);
        using var refresh = await RefreshAsync(current, tokens.RefreshToken, ct);
        refresh.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Theory(DisplayName = "PUT user should roll back authorization and sessions when revocation is canceled")]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task PutUser_Should_RollBackAuthorizationAndSessions_When_RevocationIsCanceled(
        bool changeClaims, bool removeConfirmation)
    {
        var ct = TestContext.Current.CancellationToken;
        var (user, roleId) = await CreateUserAsync(assigned: false, claims: null, ct: ct);
        using var current = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        using var other = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var tokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(current, Email, Password, ct);
        var otherTokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(other, Email, Password, ct);
        var before = await UserDatabaseTestHelper.FindAsync(Fixture, user.Id, ct);
        using var cancellation = new CancellationTokenSource();
        var interceptor = new CancelAfterRevocationInterceptor(user.Id.ToString(), cancellation);
        await using var baseFactory = new FunctionalTestWebApplicationFactory();
        await using var factory = baseFactory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.AddSingleton<IInterceptor>(interceptor)));
        using var client = new HttpClient(
            factory.Server.CreateHandler(context => context.RequestAborted = cancellation.Token))
        {
            BaseAddress = new Uri("https://localhost")
        };

        using var response = await UpdateAsync(
            client: client,
            userId: user.Id,
            roles: changeClaims || removeConfirmation ? [] : [roleId],
            accessToken: tokens.AccessToken,
            status: "Active",
            isConfirmed: !removeConfirmation,
            claims: changeClaims
                ? [new("permission", "identity.users.update"), new("permission", "identity.roles.read")]
                : null,
            ct: ct);

        ((int)response.StatusCode).ShouldBe(499, await response.Content.ReadAsStringAsync(ct));
        response.Headers.Contains("X-Session-Refresh-Required").ShouldBeFalse();
        interceptor.SavedInTransaction.ShouldBeTrue();
        var after = await UserDatabaseTestHelper.FindAsync(Fixture, user.Id, ct);
        after!.EmailConfirmed.ShouldBeTrue();
        after.SecurityStamp.ShouldBe(before!.SecurityStamp);
        after.Claims.Select(claim => claim.ClaimValue).ShouldBe(["identity.users.update"]);
        (await new UserApiTestClient(Fixture).GetDetailsAsync(user.Id, ct)).RoleIds.ShouldBeEmpty();
        await AssertAccessAsync(
            client: current,
            token: tokens.AccessToken,
            status: HttpStatusCode.OK,
            path: "/connect/userinfo",
            ct: ct);
        await AssertAccessAsync(
            client: other,
            token: otherTokens.AccessToken,
            status: HttpStatusCode.OK,
            path: "/connect/userinfo",
            ct: ct);
        await AssertCookieAsync(current, true, ct);
        await AssertCookieAsync(other, true, ct);
        using var retry = await UpdateAsync(
            client: current,
            userId: user.Id,
            roles: changeClaims || removeConfirmation ? [] : [roleId],
            accessToken: tokens.AccessToken,
            status: "Active",
            isConfirmed: !removeConfirmation,
            claims: changeClaims
                ? [new("permission", "identity.users.update"), new("permission", "identity.roles.read")]
                : null,
            ct: ct);
        retry.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Theory(DisplayName = "Access token should reject stale permissions when concurrent refresh finishes after update")]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task PutUser_Should_RejectStaleAccess_When_ConcurrentRefreshFinishesAfterUpdate(
        bool changeClaims, bool removeConfirmation)
    {
        var ct = TestContext.Current.CancellationToken;
        var (user, roleId) = await CreateUserAsync(
            assigned: !changeClaims,
            claims: changeClaims
                ? [new("permission", "identity.users.update"), new("permission", "identity.roles.read")]
                : null,
            ct: ct);
        var paused = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var resume = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var pauseRefresh = false;
        await using var baseFactory = new FunctionalTestWebApplicationFactory();
        await using var factory = baseFactory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.AddOpenIddict().AddServer(options =>
                options.AddEventHandler<OpenIddictServerEvents.ProcessSignInContext>(handler => handler
                    .SetOrder(OpenIddictServerHandlers.GenerateAccessToken.Descriptor.Order - 500)
                    .UseInlineHandler(async context =>
                    {
                        if (pauseRefresh && context.Request.IsRefreshTokenGrantType())
                        {
                            pauseRefresh = false;
                            paused.SetResult();
                            await resume.Task.WaitAsync(context.CancellationToken);
                        }
                    })))));
        using var current = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
        var tokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(current, Email, Password, ct);
        pauseRefresh = true;
        var pending = RefreshAsync(current, tokens.RefreshToken, ct);
        try
        {
            await paused.Task.WaitAsync(TimeSpan.FromSeconds(30), ct);
            using var update = await UpdateAsync(
                client: current,
                userId: user.Id,
                roles: removeConfirmation ? [roleId] : [],
                accessToken: tokens.AccessToken,
                status: "Active",
                isConfirmed: !removeConfirmation,
                claims: null,
                ct: ct);
            update.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        }
        finally
        {
            resume.TrySetResult();
        }

        using var refresh = await pending;
        refresh.StatusCode.ShouldBe(HttpStatusCode.OK, await refresh.Content.ReadAsStringAsync(ct));
        using var document = JsonDocument.Parse(await refresh.Content.ReadAsStringAsync(ct));
        await AssertAccessAsync(
            current,
            document.RootElement.GetProperty("access_token").GetString()!,
            HttpStatusCode.Unauthorized,
            "/api/v1/roles",
            ct);
        using var nextRefresh = await RefreshAsync(
            current, document.RootElement.GetProperty("refresh_token").GetString()!, ct);
        nextRefresh.StatusCode.ShouldBe(removeConfirmation ? HttpStatusCode.BadRequest : HttpStatusCode.OK);
        if (!removeConfirmation)
        {
            using var nextDocument = JsonDocument.Parse(await nextRefresh.Content.ReadAsStringAsync(ct));
            await AssertAccessAsync(
                current,
                nextDocument.RootElement.GetProperty("access_token").GetString()!,
                HttpStatusCode.Forbidden,
                "/api/v1/roles",
                ct);
        }
    }

    private static async Task<HttpResponseMessage> RefreshAsync(
        HttpClient client, string refreshToken, CancellationToken ct)
    {
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["client_id"] = OAuthAuthorizationRequestTestHelper.ClientId,
            ["refresh_token"] = refreshToken
        });
        return await client.PostAsync("/connect/token", content, ct);
    }

    private static async Task AssertAccessAsync(
        HttpClient client,
        string token,
        HttpStatusCode status,
        string path,
        CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await client.SendAsync(request, ct);
        response.StatusCode.ShouldBe(status, await response.Content.ReadAsStringAsync(ct));
    }

    private static async Task AssertCookieAsync(HttpClient client, bool valid, CancellationToken ct)
    {
        var path = await OAuthAuthorizationRequestTestHelper.BuildAuthorizePathFromParAsync(
            client: client,
            scopes: OAuthAuthorizationRequestTestHelper.DefaultScopes,
            redirectUri: "https://localhost:5173/oauth/callback",
            cancellationToken: ct);
        using var response = await client.GetAsync(path, ct);
        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        response.Headers.Location!.AbsoluteUri.ShouldStartWith(valid
            ? OAuthAuthorizationRequestTestHelper.RedirectUri
            : "https://localhost:5173/login");
    }

    private async Task<(CreateUserResponse User, Guid RoleId)> CreateUserAsync(
        bool assigned,
        IReadOnlyCollection<Claim>? claims,
        CancellationToken ct)
    {
        var role = await new RoleApiTestClient(Fixture).CreateAsync(
            new CreateRoleRequest("SessionRole", false, [new("permission", "identity.roles.read")]),
            ct);
        var user = await new UserApiTestClient(Fixture).CreateAsync(
            UserApiTestClient.CreateDefaultRequest(Email, "role-session") with
            {
                Claims = claims ?? [new("permission", "identity.users.update")],
                RoleIds = assigned ? [role.Id] : []
            },
            ct);
        return (user, role.Id);
    }

    private async Task<HttpResponseMessage> UpdateAsync(
        HttpClient client,
        Guid userId,
        IReadOnlyCollection<Guid> roles,
        string? accessToken,
        string status,
        bool isConfirmed,
        IReadOnlyCollection<Claim>? claims,
        CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/users/{userId}")
        {
            Content = JsonContent.Create(new UpdateUserRequest(
                Email,
                "role-session",
                isConfirmed,
                claims ?? [new("permission", "identity.users.update")],
                status,
                roles))
        };
        if (accessToken is null)
        {
            return await OAuthServiceAccessTokenTestHelper.SendAsync(Fixture, request, ct);
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return await client.SendAsync(request, ct);
    }

    private sealed class CancelAfterRevocationInterceptor(string subject, CancellationTokenSource cancellation)
        : SaveChangesInterceptor
    {
        internal bool SavedInTransaction { get; private set; }

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken)
        {
            if (eventData.Context?.ChangeTracker.Entries<OpenIddictEntityFrameworkCoreAuthorization<Guid>>()
                .Any(entry => entry.Entity.Subject == subject
                    && entry.Entity.Status == OpenIddictConstants.Statuses.Revoked) == true)
            {
                SavedInTransaction = eventData.Context.Database.CurrentTransaction is not null;
                await cancellation.CancelAsync();
            }

            return result;
        }
    }
}
