using System.Text.Json;

using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;
using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Roles;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Create;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Users.Create;

public sealed class CreateUserEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "POST users should create a user in PostgreSQL when request is valid")]
    public async Task PostUsers_Should_PersistUser_When_RequestIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new UserApiTestClient(Fixture);
        var request = new CreateUserRequest(
            " HERO@Example.COM ",
            " hero ",
            "StrongPassword1!",
            true,
            [new Claim("permission", "compendium.heroes.read")],
            "Active",
            RoleIds: []);

        var response = await client.CreateAsync(cancellationToken, request);
        var user = await UserDatabaseTestHelper.FindAsync(
            Fixture,
            response.Id,
            cancellationToken);

        response.Id.ShouldNotBe(Guid.Empty);
        user.ShouldNotBeNull();
        user.Email.ShouldBe("hero@example.com");
        user.UserName.ShouldBe("hero");
        user.EmailConfirmed.ShouldBeTrue();
        user.Claims.ShouldHaveSingleItem().ClaimValue.ShouldBe("compendium.heroes.read");
    }

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

    [Theory(DisplayName = "POST users should reject invalid role selections when role selection is invalid")]
    [InlineData("omitted")]
    [InlineData("null")]
    [InlineData("missing")]
    [InlineData("empty")]
    [InlineData("duplicate")]
    public async Task PostUsers_Should_ReturnBadRequest_When_RoleSelectionIsInvalid(string invalid)
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
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/users")
        {
            Content = JsonContent.Create(payload, options: JsonOptions)
        };

        using var response = await OAuthServiceAccessTokenTestHelper.SendAsync(Fixture, request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest, body);
    }
}
