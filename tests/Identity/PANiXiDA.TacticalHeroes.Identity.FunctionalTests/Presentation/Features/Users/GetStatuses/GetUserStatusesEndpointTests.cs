using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Users.GetStatuses;

public sealed class GetUserStatusesEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "GET user statuses should return all supported statuses")]
    public async Task GetUserStatuses_Should_ReturnAllStatuses_When_Authorized()
    {
        var client = new UserApiTestClient(Fixture);

        var response = await client.GetStatusesAsync(TestContext.Current.CancellationToken);

        response.ShouldContain(status =>
            status.Id == UserStatus.Active.Id &&
            status.Name == UserStatus.Active.Name);
        response.ShouldContain(status =>
            status.Id == UserStatus.Blocked.Id &&
            status.Name == UserStatus.Blocked.Name);
    }
}
