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
        role.Name.ShouldBe("CustomManager");
        role.Claims.ShouldHaveSingleItem().ClaimValue.ShouldBe("compendium.heroes.update");
    }
}
