using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using OpenIddict.Abstractions;
using OpenIddict.Server;

using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Update;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Roles.Update;

public sealed class UpdateRoleEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Theory(DisplayName = "PUT role should refresh affected users when role authorization changes")]
    [InlineData("add", "self")]
    [InlineData("remove", "self")]
    [InlineData("rename", "self")]
    [InlineData("add", "other")]
    [InlineData("remove", "other")]
    [InlineData("rename", "other")]
    [InlineData("remove", "service")]
    public async Task PutRole_Should_RefreshAffectedUsers_When_RoleAuthorizationChanges(string change, string actor)
    {
        var ct = TestContext.Current.CancellationToken;
        using var sessions = new RoleSessionTestHelper(Fixture);
        await sessions.InitializeAsync(change != "add", ct);
        var name = change == "rename" ? "RenamedRole" : "SessionRole";
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/roles/{sessions.RoleId}")
        {
            Content = JsonContent.Create(new UpdateRoleRequest(
                name, false, change == "remove" ? [] : [new("permission", "identity.roles.read")]))
        };

        using var response = await sessions.SendAsync(request, actor == "self", ct, service: actor == "service");

        await sessions.AssertRevokedAsync(response, actor == "self", change != "remove", name.ToLowerInvariant(), ct);
    }

    [Theory(DisplayName = "PUT role should preserve sessions when authorization is unchanged or update fails")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PutRole_Should_PreserveSessions_When_AuthorizationIsUnchangedOrUpdateFails(bool invalid)
    {
        var ct = TestContext.Current.CancellationToken;
        using var sessions = new RoleSessionTestHelper(Fixture);
        await sessions.InitializeAsync(true, ct);
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/roles/{sessions.RoleId}")
        {
            Content = JsonContent.Create(new UpdateRoleRequest(
                invalid ? "Developer" : "SessionRole",
                false,
                [new("permission", "identity.roles.read"), new("permission", "identity.roles.read")]))
        };

        using var response = await sessions.SendAsync(request, true, ct);

        response.StatusCode.ShouldBe(invalid ? HttpStatusCode.Conflict : HttpStatusCode.NoContent);
        response.Headers.Contains("X-Session-Refresh-Required").ShouldBeFalse();
        await RoleSessionTestHelper.AssertAccessAsync(
            sessions.Current, sessions.CurrentTokens.AccessToken, HttpStatusCode.OK, ct);
        await RoleSessionTestHelper.AssertCookieAsync(sessions.Current, true, ct);
        using var refresh = await RoleSessionTestHelper.RefreshAsync(
            sessions.Other, sessions.OtherTokens.RefreshToken, ct);
        refresh.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "PUT role should preserve refresh when current cookie is absent")]
    public async Task PutRole_Should_PreserveRefresh_When_CurrentCookieIsAbsent()
    {
        var ct = TestContext.Current.CancellationToken;
        using var sessions = new RoleSessionTestHelper(Fixture);
        await sessions.InitializeAsync(true, ct);
        using var client = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/roles/{sessions.RoleId}")
        {
            Content = JsonContent.Create(new UpdateRoleRequest("RenamedRole", false, []))
        };
        request.Headers.Authorization = new("Bearer", sessions.CurrentTokens.AccessToken);

        using var response = await client.SendAsync(request, ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        response.Headers.GetValues("X-Session-Refresh-Required").ShouldBe(["true"]);
        response.Headers.Contains("Set-Cookie").ShouldBeFalse();
        await RoleSessionTestHelper.AssertCookieAsync(sessions.Current, false, ct);
        using var refresh = await RoleSessionTestHelper.RefreshAsync(client, sessions.CurrentTokens.RefreshToken, ct);
        refresh.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "PUT role should reject stale access when concurrent refresh finishes after update")]
    public async Task PutRole_Should_RejectStaleAccess_When_ConcurrentRefreshFinishesAfterUpdate()
    {
        var ct = TestContext.Current.CancellationToken;
        using var sessions = new RoleSessionTestHelper(Fixture);
        await sessions.InitializeAsync(true, ct);
        var paused = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var resume = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var pauseRefresh = true;
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
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });

        var pending = RoleSessionTestHelper.RefreshAsync(client, sessions.CurrentTokens.RefreshToken, ct);
        try
        {
            await paused.Task.WaitAsync(TimeSpan.FromSeconds(30), ct);
            using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/roles/{sessions.RoleId}")
            {
                Content = JsonContent.Create(new UpdateRoleRequest("SessionRole", false, []))
            };
            using var response = await sessions.SendAsync(request, true, ct);
            response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        }
        finally
        {
            resume.TrySetResult();
        }

        using var refresh = await pending;
        refresh.StatusCode.ShouldBe(HttpStatusCode.OK, await refresh.Content.ReadAsStringAsync(ct));
        using var document = JsonDocument.Parse(await refresh.Content.ReadAsStringAsync(ct));
        await RoleSessionTestHelper.AssertAccessAsync(
            client,
            document.RootElement.GetProperty("access_token").GetString()!,
            HttpStatusCode.Unauthorized,
            ct);
        using var next = await RoleSessionTestHelper.RefreshAsync(
            client, document.RootElement.GetProperty("refresh_token").GetString()!, ct);
        next.StatusCode.ShouldBe(HttpStatusCode.OK);
        using var nextDocument = JsonDocument.Parse(await next.Content.ReadAsStringAsync(ct));
        await RoleSessionTestHelper.AssertAccessAsync(
            client,
            nextDocument.RootElement.GetProperty("access_token").GetString()!,
            HttpStatusCode.Forbidden,
            ct,
            "/api/v1/roles");
    }

    [Fact(DisplayName = "PUT role should update persisted role state when request is valid")]
    public async Task PutRole_Should_UpdatePostgreSql_When_RequestIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new RoleApiTestClient(Fixture);
        var createdRole = await client.RecreateAsync(cancellationToken);

        await client.UpdateAsync(
            createdRole.Id,
            new UpdateRoleRequest(
                "CustomManager",
                false,
                [new Claim("permission", "compendium.heroes.update")]),
            cancellationToken);
        var role = await RoleDatabaseTestHelper.FindAsync(
            Fixture,
            createdRole.Id,
            cancellationToken);

        role.ShouldNotBeNull();
        role.Name.ShouldBe("custommanager");
        role.Claims.ShouldHaveSingleItem().ClaimValue.ShouldBe("compendium.heroes.update");
    }

    [Fact(DisplayName = "PUT role should assign the default when previous flag is cleared")]
    public async Task PutRole_Should_AssignDefault_When_PreviousFlagIsCleared()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new RoleApiTestClient(Fixture);
        var roles = await client.GetListAsync(cancellationToken);
        var previous = roles.Items.Single(role => role.IsDefault);
        var id = roles.Items.Single(role => role.Name == "Moderator").Id;
        await client.UpdateAsync(previous.Id, new UpdateRoleRequest(previous.Name, false, []), cancellationToken);

        await client.UpdateAsync(id, new UpdateRoleRequest("CustomDefault", true, []), cancellationToken);

        var details = await client.GetDetailsAsync(id, cancellationToken);
        details.Name.ShouldBe("customdefault");
        details.IsDefault.ShouldBeTrue();
        (await client.GetListAsync(cancellationToken)).Items.Single(role => role.IsDefault).Id.ShouldBe(id);
        (await client.GetDetailsAsync(previous.Id, cancellationToken)).IsDefault.ShouldBeFalse();
    }

    [Fact(DisplayName = "PUT role should return conflict when another default exists")]
    public async Task PutRole_Should_ReturnConflict_When_AnotherDefaultExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new RoleApiTestClient(Fixture);
        var roles = await client.GetListAsync(cancellationToken);
        var id = roles.Items.Single(role => role.Name == "Moderator").Id;
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/roles/{id}")
        {
            Content = JsonContent.Create(new UpdateRoleRequest("CustomDefault", true, []), options: JsonOptions)
        };

        using var response = await OAuthServiceAccessTokenTestHelper.SendAsync(Fixture, request, cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        var existing = await client.GetListAsync(cancellationToken);
        existing.TotalCount.ShouldBe(3);
        existing.Items.Single(role => role.IsDefault).Name.ShouldBe("Player");
        (await client.GetDetailsAsync(id, cancellationToken)).Name.ShouldBe("Moderator");
    }

    [Theory(DisplayName = "PUT role should return bad request when flag is missing or null")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PutRole_Should_ReturnBadRequest_When_FlagIsMissingOrNull(bool omit)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new RoleApiTestClient(Fixture);
        var roles = await client.GetListAsync(cancellationToken);
        var id = roles.Items.Single(role => role.Name == "Moderator").Id;
        var payload = JsonSerializer.SerializeToNode(new UpdateRoleRequest("CustomDefault", false, []), JsonOptions)!
            .AsObject();
        if (omit)
        {
            payload.Remove("isDefault");
        }
        else
        {
            payload["isDefault"] = null;
        }
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/roles/{id}")
        {
            Content = JsonContent.Create(payload, options: JsonOptions)
        };

        using var response = await OAuthServiceAccessTokenTestHelper.SendAsync(Fixture, request, cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await client.GetDetailsAsync(id, cancellationToken)).Name.ShouldBe("Moderator");
    }

    [Fact(DisplayName = "PUT role should reject empty names when name is empty")]
    public async Task PutRole_Should_ReturnBadRequest_When_NameIsEmpty()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new RoleApiTestClient(Fixture);
        var roles = await client.GetListAsync(cancellationToken);
        var roleId = roles.Items.Single(role => role.Name == "Player").Id;
        var path = $"/api/v1/roles/{roleId}";
        using var request = new HttpRequestMessage(HttpMethod.Put, path)
        {
            Content = JsonContent.Create(new UpdateRoleRequest(" ", false, []), options: JsonOptions)
        };

        using var response = await OAuthServiceAccessTokenTestHelper.SendAsync(Fixture, request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var details = await client.GetDetailsAsync(roleId, cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest, body);
        details.Name.ShouldBe("Player");
    }

    [Theory(DisplayName = "PUT role should reject unknown managed claims when managed claim is unknown")]
    [InlineData("permission")]
    [InlineData("section")]
    [InlineData("custom")]
    public async Task PutRole_Should_ReturnBadRequest_When_ManagedClaimIsUnknown(string type)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new RoleApiTestClient(Fixture);
        var existing = await client.RecreateAsync(cancellationToken);
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/roles/{existing.Id}")
        {
            Content = JsonContent.Create(
                new UpdateRoleRequest("Moderator", false, [new Claim(type, "unknown")]),
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

    [Theory(DisplayName = "PUT role should return expected error when target role is missing")]
    [InlineData(false, HttpStatusCode.NotFound)]
    [InlineData(true, HttpStatusCode.Conflict)]
    public async Task PutRole_Should_ReturnExpectedError_When_TargetRoleIsMissing(
        bool isDefault,
        HttpStatusCode expectedStatusCode)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new RoleApiTestClient(Fixture);
        var roles = await client.GetListAsync(cancellationToken);
        var defaultRole = roles.Items.Single(role => role.IsDefault);
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/roles/{Guid.CreateVersion7()}")
        {
            Content = JsonContent.Create(new UpdateRoleRequest("MissingRole", isDefault, []), options: JsonOptions)
        };

        using var response = await OAuthServiceAccessTokenTestHelper.SendAsync(Fixture, request, cancellationToken);

        response.StatusCode.ShouldBe(expectedStatusCode);
        var existing = await client.GetListAsync(cancellationToken);
        existing.TotalCount.ShouldBe(roles.TotalCount);
        existing.Items.Single(role => role.IsDefault).Id.ShouldBe(defaultRole.Id);
    }
}
