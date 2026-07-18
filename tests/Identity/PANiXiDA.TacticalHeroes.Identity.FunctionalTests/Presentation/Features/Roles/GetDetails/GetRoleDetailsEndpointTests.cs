namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Roles.GetDetails;

public sealed class GetRoleDetailsEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "GET role details should return persisted role state")]
    public async Task GetRoleDetails_Should_ReturnRole_When_RoleExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new RoleApiTestClient(Fixture);
        var createdRole = await client.CreateAsync(cancellationToken);

        var response = await client.GetDetailsAsync(createdRole.Id, cancellationToken);

        response.Id.ShouldBe(createdRole.Id);
        response.Name.ShouldBe("admin");
    }
}
