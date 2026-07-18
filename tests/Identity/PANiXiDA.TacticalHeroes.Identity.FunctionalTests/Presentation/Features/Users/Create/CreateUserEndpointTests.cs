using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Create;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Users.Create;

public sealed class CreateUserEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "POST users should create a user in PostgreSQL")]
    public async Task PostUsers_Should_PersistUser_When_RequestIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new UserApiTestClient(Fixture);
        var request = new CreateUserRequest(
            " HERO@Example.COM ",
            " hero ",
            "StrongPassword1!",
            true,
            [new Claim("permission", "heroes.read")],
            "Active");

        var response = await client.CreateAsync(cancellationToken, request);
        var user = await UserDatabaseTestHelper.FindAsync(
            Fixture,
            response.Id,
            cancellationToken);

        response.Id.ShouldNotBe(Guid.Empty);
        user.ShouldNotBeNull();
        user.Email.ShouldBe("hero@example.com");
        user.UserName.ShouldBe("hero");
        user.EmailConfirmed.ShouldBeTrue();
        user.Claims.ShouldHaveSingleItem().ClaimValue.ShouldBe("heroes.read");
    }
}
