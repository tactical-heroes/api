using System.Net;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation;

public sealed class ProtectedEndpointGroupTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Theory(DisplayName = "Protected endpoint group should require authorization")]
    [InlineData("/api/v1/accounts")]
    [InlineData("/api/v1/roles")]
    public async Task Get_Should_ReturnUnauthorized_When_EndpointGroupIsProtected(string path)
    {
        using var response = await Client.GetAsync(
            path,
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
