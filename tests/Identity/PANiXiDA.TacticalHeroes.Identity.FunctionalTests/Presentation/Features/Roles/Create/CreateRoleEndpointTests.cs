using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Create;

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
                [new Claim("permission", "compendium.heroes.update")]));
        var role = await RoleDatabaseTestHelper.FindAsync(
            Fixture,
            response.Id,
            cancellationToken);

        role.ShouldNotBeNull();
        role.Name.ShouldBe("Moderator");
        role.Claims.ShouldHaveSingleItem().ClaimValue.ShouldBe("compendium.heroes.update");
    }
}
