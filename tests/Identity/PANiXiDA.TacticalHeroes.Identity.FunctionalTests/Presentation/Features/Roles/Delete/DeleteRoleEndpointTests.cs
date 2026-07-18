namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Roles.Delete;

public sealed class DeleteRoleEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "DELETE role should remove the role from PostgreSQL")]
    public async Task DeleteRole_Should_RemoveRole_When_RoleExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new RoleApiTestClient(Fixture);
        var createdRole = await client.CreateAsync(cancellationToken);

        await client.DeleteAsync(createdRole.Id, cancellationToken);
        var role = await RoleDatabaseTestHelper.FindAsync(
            Fixture,
            createdRole.Id,
            cancellationToken);

        role.ShouldBeNull();
    }
}
