using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;
using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Users;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Register;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Create;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Update;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Roles;

public sealed class DefaultRoleEndpointTests(FunctionalTestFixture fixture) : FunctionalTestBase(fixture)
{
    [Theory(DisplayName = "Role requests should switch the default when custom role is selected")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SaveRole_Should_SwitchDefault_When_CustomRoleIsSelected(bool create)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new RoleApiTestClient(Fixture);
        var initialRoles = await client.GetListAsync(cancellationToken);
        var previous = initialRoles.Items.Single(role => role.IsDefault);
        previous.Name.ShouldBe("Player");
        Guid id;
        if (create)
        {
            id = (await client.CreateAsync(
                cancellationToken,
                new CreateRoleRequest("CustomDefault", [], IsDefault: true))).Id;
        }
        else
        {
            id = initialRoles.Items.Single(role => role.Name == "Moderator").Id;
            await client.UpdateAsync(
                id, new UpdateRoleRequest("CustomDefault", [], IsDefault: true), cancellationToken);
        }

        await client.UpdateAsync(id, new UpdateRoleRequest("RenamedDefault", [], IsDefault: true), cancellationToken);

        var roles = await client.GetListAsync(cancellationToken);
        roles.Items.Single(role => role.IsDefault).Id.ShouldBe(id);
        var details = await client.GetDetailsAsync(id, cancellationToken);
        details.Name.ShouldBe("RenamedDefault");
        details.IsDefault.ShouldBeTrue();
        (await client.GetDetailsAsync(previous.Id, cancellationToken)).IsDefault.ShouldBeFalse();
        using var response = await Client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterUserRequest("default@example.com", "new-user", "StrongPassword1!"),
            JsonOptions, cancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var registered = await response.Content.ReadFromJsonAsync<RegisterUserResponse>(JsonOptions, cancellationToken);
        registered.ShouldNotBeNull();
        var user = await UserDatabaseTestHelper.FindAsync(Fixture, registered.Id, cancellationToken);
        user.ShouldNotBeNull();
        user.Roles.ShouldHaveSingleItem().RoleId.ShouldBe(id);
    }

    [Theory(DisplayName = "Role requests should return conflict when current default would be removed")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SaveRole_Should_ReturnConflict_When_CurrentDefaultWouldBeRemoved(bool delete)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new RoleApiTestClient(Fixture);
        var roles = await client.GetListAsync(cancellationToken);
        var current = roles.Items.Single(role => role.IsDefault);
        using var request = new HttpRequestMessage(
            delete ? HttpMethod.Delete : HttpMethod.Put,
            $"/api/v1/roles/{current.Id}");
        if (!delete)
        {
            request.Content = JsonContent.Create(
                new UpdateRoleRequest("Player", [], IsDefault: false),
                options: JsonOptions);
        }

        using var response = await OAuthServiceAccessTokenTestHelper.SendAsync(Fixture, request, cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await client.GetDetailsAsync(current.Id, cancellationToken)).IsDefault.ShouldBeTrue();
    }
}
