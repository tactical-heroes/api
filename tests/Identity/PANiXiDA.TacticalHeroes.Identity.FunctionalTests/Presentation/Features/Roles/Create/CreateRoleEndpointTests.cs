using System.Net.Http.Headers;
using System.Text.Json;

using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Create;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Update;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Roles.Create;

public sealed class CreateRoleEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "POST roles should create a role in PostgreSQL when request is valid")]
    public async Task PostRoles_Should_PersistRole_When_RequestIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new RoleApiTestClient(Fixture);

        var response = await client.RecreateAsync(
            cancellationToken,
            new CreateRoleRequest(
                " Moderator ",
                false,
                [new Claim("permission", "compendium.heroes.update")]));
        var role = await RoleDatabaseTestHelper.FindAsync(
            Fixture,
            response.Id,
            cancellationToken);

        role.ShouldNotBeNull();
        role.Name.ShouldBe("moderator");
        role.Claims.ShouldHaveSingleItem().ClaimValue.ShouldBe("compendium.heroes.update");
    }

    [Fact(DisplayName = "POST roles should assign the default when previous flag is cleared")]
    public async Task PostRoles_Should_AssignDefault_When_PreviousFlagIsCleared()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new RoleApiTestClient(Fixture);
        var roles = await client.GetListAsync(cancellationToken);
        var previous = roles.Items.Single(role => role.IsDefault);
        await client.UpdateAsync(previous.Id, new UpdateRoleRequest(previous.Name, false, []), cancellationToken);

        var created = await client.CreateAsync(
            cancellationToken, new CreateRoleRequest("CustomDefault", true, []));

        var details = await client.GetDetailsAsync(created.Id, cancellationToken);
        details.IsDefault.ShouldBeTrue();
        (await client.GetListAsync(cancellationToken)).Items.Single(role => role.IsDefault).Id.ShouldBe(created.Id);
        (await client.GetDetailsAsync(previous.Id, cancellationToken)).IsDefault.ShouldBeFalse();
    }

    [Fact(DisplayName = "POST roles should return conflict when another default exists")]
    public async Task PostRoles_Should_ReturnConflict_When_AnotherDefaultExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new RoleApiTestClient(Fixture);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/roles")
        {
            Content = JsonContent.Create(new CreateRoleRequest("CustomDefault", true, []), options: JsonOptions)
        };

        using var response = await OAuthServiceAccessTokenTestHelper.SendAsync(Fixture, request, cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        var roles = await client.GetListAsync(cancellationToken);
        roles.TotalCount.ShouldBe(3);
        roles.Items.Single(role => role.IsDefault).Name.ShouldBe("Player");
    }

    [Theory(DisplayName = "POST roles should return bad request when flag is missing or null")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PostRoles_Should_ReturnBadRequest_When_FlagIsMissingOrNull(bool omit)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var payload = JsonSerializer.SerializeToNode(new CreateRoleRequest("CustomDefault", false, []), JsonOptions)!
            .AsObject();
        if (omit)
        {
            payload.Remove("isDefault");
        }
        else
        {
            payload["isDefault"] = null;
        }
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/roles")
        {
            Content = JsonContent.Create(payload, options: JsonOptions)
        };

        using var response = await OAuthServiceAccessTokenTestHelper.SendAsync(Fixture, request, cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await new RoleApiTestClient(Fixture).GetListAsync(cancellationToken)).TotalCount.ShouldBe(3);
    }

    [Fact(DisplayName = "POST roles should reject empty names when name is empty")]
    public async Task PostRoles_Should_ReturnBadRequest_When_NameIsEmpty()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new RoleApiTestClient(Fixture);
        var roles = await client.GetListAsync(cancellationToken);
        var roleId = roles.Items.Single(role => role.Name == "Player").Id;
        var path = "/api/v1/roles";
        using var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(new CreateRoleRequest(" ", false, []), options: JsonOptions)
        };

        using var response = await OAuthServiceAccessTokenTestHelper.SendAsync(Fixture, request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var details = await client.GetDetailsAsync(roleId, cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest, body);
        details.Name.ShouldBe("Player");
    }

    [Theory(DisplayName = "POST roles should reject unknown managed claims when managed claim is unknown")]
    [InlineData("permission")]
    [InlineData("section")]
    [InlineData("custom")]
    public async Task PostRoles_Should_ReturnBadRequest_When_ManagedClaimIsUnknown(string type)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/roles")
        {
            Content = JsonContent.Create(
                new CreateRoleRequest("Moderator", false, [new Claim(type, "unknown")]),
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
    }

    [Theory(DisplayName = "POST roles should enforce permission and claim contract when creating role")]
    [InlineData("identity.roles.read", "custom", HttpStatusCode.Forbidden)]
    [InlineData("identity.roles.create", "section", HttpStatusCode.BadRequest)]
    [InlineData("identity.roles.create", " SECTION ", HttpStatusCode.BadRequest)]
    [InlineData("identity.roles.create", "custom", HttpStatusCode.BadRequest)]
    public async Task PostRoles_Should_EnforcePermissionAndClaimContract_When_CreatingRole(
        string permission, string claimType, HttpStatusCode expected)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/roles")
        {
            Content = JsonContent.Create(
                new CreateRoleRequest(
                    "PermissionTest", false, [new(claimType, "identity")]),
                options: JsonOptions)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await OAuthServiceAccessTokenTestHelper.IssueAccessTokenAsync(Fixture, cancellationToken, [permission]));

        using var response = await Client.SendAsync(request, cancellationToken);

        response.StatusCode.ShouldBe(expected, await response.Content.ReadAsStringAsync(cancellationToken));
    }
}
