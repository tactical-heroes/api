using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;

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
}
