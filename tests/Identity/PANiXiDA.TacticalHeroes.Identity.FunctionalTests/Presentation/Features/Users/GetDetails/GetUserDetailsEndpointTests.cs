namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Users.GetDetails;

public sealed class GetUserDetailsEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "GET user details should return persisted user state")]
    public async Task GetUserDetails_Should_ReturnUser_When_UserExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new UserApiTestClient(Fixture);
        var createdUser = await client.CreateAsync(cancellationToken);

        var response = await client.GetDetailsAsync(createdUser.Id, cancellationToken);

        response.Id.ShouldBe(createdUser.Id);
        response.Email.ShouldBe("hero@example.com");
        response.UserName.ShouldBe("hero");
        response.IsConfirmed.ShouldBeTrue();
        response.Status.ShouldBe("Active");
    }
}
