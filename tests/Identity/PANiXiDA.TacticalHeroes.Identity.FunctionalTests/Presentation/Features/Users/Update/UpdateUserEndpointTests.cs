using System.Text.Json;

using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;
using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Roles;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Update;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Users.Update;

public sealed class UpdateUserEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "PUT user should update persisted user state when request is valid")]
    public async Task PutUser_Should_UpdatePostgreSql_When_RequestIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new UserApiTestClient(Fixture);
        var createdUser = await client.CreateAsync(cancellationToken);

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
            cancellationToken, UserApiTestClient.CreateDefaultRequest() with { RoleIds = [developerId] });
        var payload = JsonSerializer.SerializeToNode(
            new UpdateUserRequest(
                Email: "rejected@example.com",
                UserName: "hero",
                IsConfirmed: true,
                Claims: [],
                Status: "Active",
                RoleIds: roleIds), JsonOptions)!
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
}
