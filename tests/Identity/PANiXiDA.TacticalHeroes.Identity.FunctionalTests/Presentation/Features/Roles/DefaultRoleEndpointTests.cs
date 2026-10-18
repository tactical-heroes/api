using System.Text.Json;

using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;
using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Users;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Register;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Create;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Update;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Roles;

public sealed class DefaultRoleEndpointTests(FunctionalTestFixture fixture) : FunctionalTestBase(fixture)
{
    [Theory(DisplayName = "Role requests should assign the default when previous flag is cleared")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SaveRole_Should_AssignDefault_When_PreviousFlagIsCleared(bool create)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new RoleApiTestClient(Fixture);
        var initialRoles = await client.GetListAsync(cancellationToken);
        var previous = initialRoles.Items.Single(role => role.IsDefault);
        await client.UpdateAsync(previous.Id, new UpdateRoleRequest(previous.Name, false, []), cancellationToken);
        Guid id;
        if (create)
        {
            id = (await client.CreateAsync(
                cancellationToken,
                new CreateRoleRequest("CustomDefault", true, []))).Id;
        }
        else
        {
            id = initialRoles.Items.Single(role => role.Name == "Moderator").Id;
            await client.UpdateAsync(id, new UpdateRoleRequest("CustomDefault", true, []), cancellationToken);
        }

        await client.UpdateAsync(id, new UpdateRoleRequest("RenamedDefault", true, []), cancellationToken);

        var roles = await client.GetListAsync(cancellationToken);
        roles.Items.Single(role => role.IsDefault).Id.ShouldBe(id);
        var details = await client.GetDetailsAsync(id, cancellationToken);
        details.Name.ShouldBe("renameddefault");
        details.IsDefault.ShouldBeTrue();
        (await client.GetDetailsAsync(previous.Id, cancellationToken)).IsDefault.ShouldBeFalse();
        using var response = await Client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterUserRequest("default@example.com", "new-user", "StrongPassword1!"),
            JsonOptions,
            cancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var registered = await response.Content.ReadFromJsonAsync<RegisterUserResponse>(JsonOptions, cancellationToken);
        registered.ShouldNotBeNull();
        var user = await UserDatabaseTestHelper.FindAsync(Fixture, registered.Id, cancellationToken);
        user.ShouldNotBeNull();
        user.Roles.ShouldHaveSingleItem().RoleId.ShouldBe(id);
    }

    [Theory(DisplayName = "Role requests should return conflict when another default exists")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SaveRole_Should_ReturnConflict_When_AnotherDefaultExists(bool create)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new RoleApiTestClient(Fixture);
        var roles = await client.GetListAsync(cancellationToken);
        var id = roles.Items.Single(role => role.Name == "Moderator").Id;
        var path = create ? "/api/v1/roles" : $"/api/v1/roles/{id}";
        using var request = new HttpRequestMessage(create ? HttpMethod.Post : HttpMethod.Put, path)
        {
            Content = JsonContent.Create(new CreateRoleRequest("CustomDefault", true, []), options: JsonOptions)
        };

        using var response = await OAuthServiceAccessTokenTestHelper.SendAsync(Fixture, request, cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        var existing = await client.GetListAsync(cancellationToken);
        existing.TotalCount.ShouldBe(3);
        existing.Items.Single(role => role.IsDefault).Name.ShouldBe("Player");
        (await client.GetDetailsAsync(id, cancellationToken)).Name.ShouldBe("Moderator");
    }

    [Fact(DisplayName = "Role deletion should return conflict when role is default")]
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

    [Theory(DisplayName = "Role requests should return bad request when flag is missing or null")]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task SaveRole_Should_ReturnBadRequest_When_FlagIsMissingOrNull(bool update, bool omit)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new RoleApiTestClient(Fixture);
        var roles = await client.GetListAsync(cancellationToken);
        var id = roles.Items.Single(role => role.Name == "Moderator").Id;
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
        var path = update ? $"/api/v1/roles/{id}" : "/api/v1/roles";
        using var request = new HttpRequestMessage(update ? HttpMethod.Put : HttpMethod.Post, path)
        {
            Content = JsonContent.Create(payload, options: JsonOptions)
        };

        using var response = await OAuthServiceAccessTokenTestHelper.SendAsync(Fixture, request, cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await client.GetDetailsAsync(id, cancellationToken)).Name.ShouldBe("Moderator");
    }
}
