namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Users.Delete;

public sealed class DeleteUserEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "DELETE user should remove the user from PostgreSQL")]
    public async Task DeleteUser_Should_RemoveUser_When_UserExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new UserApiTestClient(Fixture);
        var createdUser = await client.CreateAsync(cancellationToken);

        await client.DeleteAsync(createdUser.Id, cancellationToken);
        var user = await UserDatabaseTestHelper.FindAsync(
            Fixture,
            createdUser.Id,
            cancellationToken);

        user.ShouldBeNull();
    }
}
