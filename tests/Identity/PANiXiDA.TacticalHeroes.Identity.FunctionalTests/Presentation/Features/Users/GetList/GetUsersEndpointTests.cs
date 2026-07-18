namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Users.GetList;

public sealed class GetUsersEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "GET users should return a filtered page from PostgreSQL")]
    public async Task GetUsers_Should_ReturnFilteredPage_When_EmailIsProvided()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new UserApiTestClient(Fixture);
        await client.CreateAsync(
            cancellationToken,
            UserApiTestClient.CreateDefaultRequest("first@example.com", "first-hero"));
        var secondUser = await client.CreateAsync(
            cancellationToken,
            UserApiTestClient.CreateDefaultRequest("second@example.com", "second-hero"));

        var response = await client.GetListAsync(
            cancellationToken,
            email: "second@example.com");

        response.TotalCount.ShouldBe(1);
        var user = response.Items.ShouldHaveSingleItem();
        user.Id.ShouldBe(secondUser.Id);
        user.Email.ShouldBe("second@example.com");
    }
}
