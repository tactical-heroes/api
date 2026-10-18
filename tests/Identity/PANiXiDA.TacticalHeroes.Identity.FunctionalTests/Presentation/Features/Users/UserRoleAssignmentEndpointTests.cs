using System.Text.Json;

using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;
using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Roles;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Update;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Users;

public sealed class UserRoleAssignmentEndpointTests(FunctionalTestFixture fixture) : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "User creation should assign multiple roles when roles are selected")]
    public async Task PostUsers_Should_AssignMultipleRoles_When_RolesAreSelected()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var roles = (await new RoleApiTestClient(Fixture).GetListAsync(cancellationToken)).Items;
        Guid[] selected =
        [
            roles.Single(role => role.Name == "Developer").Id,
            roles.Single(role => role.Name == "Moderator").Id
        ];
        var client = new UserApiTestClient(Fixture);

        var created = await client.CreateAsync(
            cancellationToken, UserApiTestClient.CreateDefaultRequest() with { RoleIds = selected });
        var details = await client.GetDetailsAsync(created.Id, cancellationToken);

        details.RoleIds.ShouldBe(selected, ignoreOrder: true);
        var user = await UserDatabaseTestHelper.FindAsync(Fixture, created.Id, cancellationToken);
        user.ShouldNotBeNull();
        user.Roles.Select(role => role.RoleId).ShouldBe(selected, ignoreOrder: true);
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
            cancellationToken, UserApiTestClient.CreateDefaultRequest() with { RoleIds = [developerId] });
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

    [Theory(DisplayName = "User requests should reject invalid role selections when role selection is invalid")]
    [InlineData(false, "omitted")]
    [InlineData(true, "omitted")]
    [InlineData(false, "null")]
    [InlineData(true, "null")]
    [InlineData(false, "missing")]
    [InlineData(true, "missing")]
    [InlineData(false, "empty")]
    [InlineData(true, "empty")]
    [InlineData(false, "duplicate")]
    [InlineData(true, "duplicate")]
    public async Task SaveUser_Should_ReturnBadRequest_When_RoleSelectionIsInvalid(bool update, string invalid)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var roles = (await new RoleApiTestClient(Fixture).GetListAsync(cancellationToken)).Items;
        var developerId = roles.Single(role => role.Name == "Developer").Id;
        Guid[] roleIds = invalid switch
        {
            "empty" => [Guid.Empty],
            "duplicate" => [developerId, developerId],
            _ => [developerId, Guid.CreateVersion7()]
        };
        var payload = JsonSerializer.SerializeToNode(
            UserApiTestClient.CreateDefaultRequest() with { RoleIds = roleIds }, JsonOptions)!
            .AsObject();
        if (invalid == "omitted")
        {
            payload.Remove("roleIds");
        }
        else if (invalid == "null")
        {
            payload["roleIds"] = null;
        }
        var path = "/api/v1/users";
        Guid? existingId = null;
        if (update)
        {
            var client = new UserApiTestClient(Fixture);
            var user = await client.CreateAsync(
                cancellationToken, UserApiTestClient.CreateDefaultRequest() with { RoleIds = [developerId] });
            existingId = user.Id;
            path = $"{path}/{user.Id}";
            payload.Remove("password");
            payload["email"] = "rejected@example.com";
        }
        using var request = new HttpRequestMessage(update ? HttpMethod.Put : HttpMethod.Post, path)
        {
            Content = JsonContent.Create(payload, options: JsonOptions)
        };

        using var response = await OAuthServiceAccessTokenTestHelper.SendAsync(Fixture, request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest, body);
        if (existingId is not null)
        {
            var details = await new UserApiTestClient(Fixture).GetDetailsAsync(existingId.Value, cancellationToken);
            details.Email.ShouldBe("hero@example.com");
            details.RoleIds.ShouldBe([developerId]);
        }
    }
}
