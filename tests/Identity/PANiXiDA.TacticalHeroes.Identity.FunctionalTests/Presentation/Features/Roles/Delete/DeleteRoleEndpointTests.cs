using System.Net.Http.Headers;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;
using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Users;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Create;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Roles.Delete;

public sealed class DeleteRoleEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "DELETE role should remove the role from PostgreSQL when role exists")]
    public async Task DeleteRole_Should_RemoveRole_When_RoleExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new RoleApiTestClient(Fixture);
        var createdRole = await client.RecreateAsync(cancellationToken);

        await client.DeleteAsync(createdRole.Id, cancellationToken);
        var role = await RoleDatabaseTestHelper.FindAsync(
            Fixture,
            createdRole.Id,
            cancellationToken);

        role.ShouldBeNull();
    }

    [Fact(DisplayName = "DELETE role should return conflict when role is default")]
    public async Task DeleteRole_Should_ReturnConflict_When_RoleIsDefault()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new RoleApiTestClient(Fixture);
        var roles = await client.GetListAsync(cancellationToken);
        var current = roles.Items.Single(role => role.IsDefault);
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/roles/{current.Id}");

        using var response = await OAuthServiceAccessTokenTestHelper.SendAsync(Fixture, request, cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await client.GetDetailsAsync(current.Id, cancellationToken)).IsDefault.ShouldBeTrue();
    }

    [Fact(DisplayName = "DELETE role should revoke access when authorization changes")]
    public async Task DeleteRole_Should_RevokeAccess_When_AuthorizationChanges()
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

        await roles.DeleteAsync(role.Id, cancellationToken);

        using var response = await userClient.GetAsync($"/api/v1/users/{user.Id}", cancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "DELETE role should roll back changes when session invalidation fails")]
    public async Task DeleteRole_Should_RollBackChanges_When_SessionInvalidationFails()
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
            HttpMethod.Delete, $"/api/v1/roles/{role.Id}");

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
