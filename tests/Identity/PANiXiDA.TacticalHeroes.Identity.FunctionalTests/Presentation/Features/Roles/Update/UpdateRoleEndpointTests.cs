using System.Text.Json;

using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;
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

    [Theory(DisplayName = "PUT role should reject unknown managed claims when managed claim is unknown")]
    [InlineData("permission")]
    [InlineData("section")]
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

    [Fact(DisplayName = "PUT role should persist custom claims when no permissions are assigned")]
    public async Task PutRole_Should_PersistCustomClaim_When_NoPermissionsAreAssigned()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new RoleApiTestClient(Fixture);
        Claim[] claims = [new("custom", "identity")];
        var role = await client.RecreateAsync(cancellationToken);

        await client.UpdateAsync(role.Id, new UpdateRoleRequest("Moderator", false, claims), cancellationToken);
        var details = await client.GetDetailsAsync(role.Id, cancellationToken);

        details.Claims.ShouldBe(claims);
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
