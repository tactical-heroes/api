using System.Net.Http.Headers;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;
using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Users;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Create;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Update;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Roles;

public sealed class RoleSessionInvalidationEndpointTests(FunctionalTestFixture fixture) : FunctionalTestBase(fixture)
{
    [Theory(DisplayName = "Role requests should revoke access when authorization changes")]
    [InlineData("claims")]
    [InlineData("name")]
    [InlineData("delete")]
    public async Task SaveRole_Should_RevokeAccess_When_AuthorizationChanges(string change)
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
        if (change == "delete")
        {
            await roles.DeleteAsync(role.Id, cancellationToken);
        }
        else
        {
            await roles.UpdateAsync(
                role.Id,
                new UpdateRoleRequest(
                    change == "name" ? "renamed" : "scoped",
                    false,
                    [new Claim("permission", permission)]),
                cancellationToken);
        }

        using var response = await userClient.GetAsync($"/api/v1/users/{user.Id}", cancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Theory(DisplayName = "Role requests should roll back changes when session invalidation fails")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SaveRole_Should_RollBackChanges_When_SessionInvalidationFails(bool delete)
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
            delete ? HttpMethod.Delete : HttpMethod.Put, $"/api/v1/roles/{role.Id}");
        if (!delete)
        {
            request.Content = JsonContent.Create(
                new UpdateRoleRequest(
                    "changed", false, [new Claim("permission", "compendium.heroes.update")]),
                options: JsonOptions);
        }

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
