using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Update;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Users.Update;

public sealed class UpdateUserEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "PUT user should update persisted user state")]
    public async Task PutUser_Should_UpdatePostgreSql_When_RequestIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new UserApiTestClient(Fixture);
        var createdUser = await client.CreateAsync(cancellationToken);

        await client.UpdateAsync(
            createdUser.Id,
            new UpdateUserRequest(
                "updated@example.com",
                "updated-hero",
                false,
                [new Claim("permission", "heroes.manage")],
                "Blocked"),
            cancellationToken);
        var user = await UserDatabaseTestHelper.FindAsync(
            Fixture,
            createdUser.Id,
            cancellationToken);

        user.ShouldNotBeNull();
        user.Email.ShouldBe("updated@example.com");
        user.UserName.ShouldBe("updated-hero");
        user.EmailConfirmed.ShouldBeFalse();
        user.Status.ShouldBe("Blocked");
        user.Claims.ShouldHaveSingleItem().ClaimValue.ShouldBe("heroes.manage");
    }
}
