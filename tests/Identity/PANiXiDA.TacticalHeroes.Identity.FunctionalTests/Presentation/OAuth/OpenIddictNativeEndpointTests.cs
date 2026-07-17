using System.Net;
using System.Text.Json;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.OAuth;

public sealed class OpenIddictNativeEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Theory(DisplayName = "Native OAuth endpoint should be handled by OpenIddict")]
    [InlineData("/connect/par")]
    [InlineData("/connect/introspect")]
    [InlineData("/connect/revoke")]
    public async Task Post_Should_BeHandledByOpenIddict_When_RequestIsInvalid(string path)
    {
        using var content = new FormUrlEncodedContent([]);
        using var response = await Client.PostAsync(
            path,
            content,
            TestContext.Current.CancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest, responseBody);

        using var document = JsonDocument.Parse(responseBody);
        document.RootElement.GetProperty("error").GetString().ShouldNotBeNullOrWhiteSpace();
    }
}
