namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Users.Unblock;

public sealed class UnblockUserEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "POST user unblock should persist active status")]
    public async Task UnblockUser_Should_PersistActiveStatus_When_UserIsBlocked()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new UserApiTestClient(Fixture);
        var createdUser = await client.CreateAsync(cancellationToken);
        await client.BlockAsync(createdUser.Id, cancellationToken);

        await client.UnblockAsync(createdUser.Id, cancellationToken);
        var user = await UserDatabaseTestHelper.FindAsync(
            Fixture,
            createdUser.Id,
            cancellationToken);

        user.ShouldNotBeNull();
        user.Status.ShouldBe("Active");
    }
}
