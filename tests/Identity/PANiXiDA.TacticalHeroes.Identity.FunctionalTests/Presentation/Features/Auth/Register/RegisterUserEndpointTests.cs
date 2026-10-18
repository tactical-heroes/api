using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Roles;
using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Users;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Register;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Create;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Update;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Auth.Register;

public sealed class RegisterUserEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "POST auth register should create an unconfirmed user in PostgreSQL when request is valid")]
    public async Task PostRegister_Should_PersistUnconfirmedUser_When_RequestIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        using var response = await Client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterUserRequest(
                " REGISTERED@Example.COM ",
                " registered-hero ",
                "StrongPassword1!"),
            JsonOptions,
            cancellationToken);
        var responseBody = await response.Content.ReadFromJsonAsync<RegisterUserResponse>(
            JsonOptions,
            cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        responseBody.ShouldNotBeNull();
        response.Headers.Location.ShouldNotBeNull();
        var user = await UserDatabaseTestHelper.FindAsync(
            Fixture,
            responseBody.Id,
            cancellationToken);
        user.ShouldNotBeNull();
        user.Email.ShouldBe("registered@example.com");
        user.UserName.ShouldBe("registered-hero");
        user.EmailConfirmed.ShouldBeFalse();
        user.Roles.ShouldHaveSingleItem().Role!.Name.ShouldBe("Player");
    }

    [Fact(DisplayName = "POST auth register should assign the configured role when default role is renamed")]
    public async Task PostRegister_Should_AssignConfiguredRole_When_DefaultRoleIsRenamed()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var roles = new RoleApiTestClient(Fixture);
        var previous = (await roles.GetListAsync(cancellationToken)).Items.Single(role => role.IsDefault);
        await roles.UpdateAsync(previous.Id, new UpdateRoleRequest(previous.Name, false, []), cancellationToken);
        var created = await roles.CreateAsync(
            cancellationToken, new CreateRoleRequest("CustomDefault", true, []));
        await roles.UpdateAsync(created.Id, new UpdateRoleRequest("RenamedDefault", true, []), cancellationToken);

        using var response = await Client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterUserRequest("default@example.com", "new-user", "StrongPassword1!"),
            JsonOptions,
            cancellationToken);
        var registered = await response.Content.ReadFromJsonAsync<RegisterUserResponse>(JsonOptions, cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        registered.ShouldNotBeNull();
        var user = await UserDatabaseTestHelper.FindAsync(Fixture, registered.Id, cancellationToken);
        user.ShouldNotBeNull();
        user.Roles.ShouldHaveSingleItem().RoleId.ShouldBe(created.Id);
        var details = await roles.GetDetailsAsync(created.Id, cancellationToken);
        details.Name.ShouldBe("renameddefault");
        details.IsDefault.ShouldBeTrue();
    }
}
