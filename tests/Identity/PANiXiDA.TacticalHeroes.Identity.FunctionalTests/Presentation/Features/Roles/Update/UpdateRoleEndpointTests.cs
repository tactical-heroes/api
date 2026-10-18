using System.Net.Http.Headers;
using System.Text.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;
using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Users;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Create;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Update;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Roles.Update;

public sealed class UpdateRoleEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
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

    [Theory(DisplayName = "PUT role should revoke access when authorization changes")]
    [InlineData("claims")]
    [InlineData("name")]
    public async Task PutRole_Should_RevokeAccess_When_AuthorizationChanges(string change)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var roles = new RoleApiTestClient(Fixture);
        var role = await roles.CreateAsync(
            cancellationToken,
            new CreateRoleRequest(
                "scoped", false, [new Claim("permission", "compendium.heroes.read")]));
        var users = new UserApiTestClient(Fixture);
        var user = await users.CreateAsync(
            cancellationToken, UserApiTestClient.CreateDefaultRequest() with { RoleIds = [role.Id] });
        using var userClient = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var tokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            userClient, "hero@example.com", "StrongPassword1!", cancellationToken);
        userClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        using var initial = await userClient.GetAsync($"/api/v1/users/{user.Id}", cancellationToken);
        initial.StatusCode.ShouldBe(HttpStatusCode.OK);

        var permission = change == "claims" ? "compendium.heroes.update" : "compendium.heroes.read";
        await roles.UpdateAsync(
            role.Id,
            new UpdateRoleRequest(
                change == "name" ? "renamed" : "scoped",
                false,
                [new Claim("permission", permission)]),
            cancellationToken);

        using var response = await userClient.GetAsync($"/api/v1/users/{user.Id}", cancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "PUT role should roll back changes when session invalidation fails")]
    public async Task PutRole_Should_RollBackChanges_When_SessionInvalidationFails()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var roles = new RoleApiTestClient(Fixture);
        var role = await roles.CreateAsync(
            cancellationToken,
            new CreateRoleRequest(
                "scoped", false, [new Claim("permission", "compendium.heroes.read")]));
        var users = new UserApiTestClient(Fixture);
        var user = await users.CreateAsync(
            cancellationToken, UserApiTestClient.CreateDefaultRequest() with { RoleIds = [role.Id] });
        await using (var scope = Fixture.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
            var stored = await dbContext.Set<ApplicationUser>()
                .SingleAsync(item => item.Id == user.Id, cancellationToken);
            stored.UserName = string.Empty;
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        var originalUser = await UserDatabaseTestHelper.FindAsync(Fixture, user.Id, cancellationToken);
        originalUser.ShouldNotBeNull();
        using var request = new HttpRequestMessage(
            HttpMethod.Put, $"/api/v1/roles/{role.Id}");
        request.Content = JsonContent.Create(
            new UpdateRoleRequest(
                "changed", false, [new Claim("permission", "compendium.heroes.update")]),
            options: JsonOptions);

        using var response = await OAuthServiceAccessTokenTestHelper.SendAsync(Fixture, request, cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var persistedRole = await roles.GetDetailsAsync(role.Id, cancellationToken);
        persistedRole.Name.ShouldBe("scoped");
        persistedRole.Claims.ShouldHaveSingleItem().Value.ShouldBe("compendium.heroes.read");
        var persistedUser = await UserDatabaseTestHelper.FindAsync(Fixture, user.Id, cancellationToken);
        persistedUser.ShouldNotBeNull();
        persistedUser.SecurityStamp.ShouldBe(originalUser.SecurityStamp);
        persistedUser.Roles.ShouldHaveSingleItem().RoleId.ShouldBe(role.Id);
    }
}
