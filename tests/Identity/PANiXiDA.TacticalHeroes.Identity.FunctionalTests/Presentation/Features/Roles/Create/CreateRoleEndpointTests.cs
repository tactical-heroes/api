using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Create;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Roles.Create;

public sealed class CreateRoleEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "POST roles should create a role in PostgreSQL")]
    public async Task PostRoles_Should_PersistRole_When_RequestIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new RoleApiTestClient(Fixture);

        var response = await client.CreateAsync(
            cancellationToken,
            new CreateRoleRequest(
                " ADMIN ",
                [new Claim("permission", "heroes.manage")]));
        var role = await RoleDatabaseTestHelper.FindAsync(
            Fixture,
            response.Id,
            cancellationToken);

        role.ShouldNotBeNull();
        role.Name.ShouldBe("admin");
        role.Claims.ShouldHaveSingleItem().ClaimValue.ShouldBe("heroes.manage");
    }
}
