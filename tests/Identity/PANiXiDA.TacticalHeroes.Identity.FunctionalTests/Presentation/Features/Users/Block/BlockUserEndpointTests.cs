namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Users.Block;

public sealed class BlockUserEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "POST user block should persist blocked status")]
    public async Task BlockUser_Should_PersistBlockedStatus_When_UserExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new UserApiTestClient(Fixture);
        var createdUser = await client.CreateAsync(cancellationToken);

        await client.BlockAsync(createdUser.Id, cancellationToken);
        var user = await UserDatabaseTestHelper.FindAsync(
            Fixture,
            createdUser.Id,
            cancellationToken);

        user.ShouldNotBeNull();
        user.Status.ShouldBe("Blocked");
    }
}
