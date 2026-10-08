using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth.OpenApi;

public sealed class ScalarFaviconTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "Scalar should use and serve the host favicon when environment is development")]
    public async Task GetScalar_Should_UseAndServeHostFavicon_When_EnvironmentIsDevelopment()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = Fixture.CreateClient(Environments.Development);

        var scalarContent = await client.GetStringAsync("/scalar", cancellationToken);
        using var response = await client.GetAsync("/favicon.ico", cancellationToken);
        var favicon = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var environment = Fixture.Services.GetRequiredService<IWebHostEnvironment>();
        var expectedFavicon = await File.ReadAllBytesAsync(
            Path.Combine(environment.ContentRootPath, "wwwroot", "favicon.ico"),
            cancellationToken);

        scalarContent.ShouldContain("\"favicon\":\"/favicon.ico\"");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("image/x-icon");
        favicon.ShouldNotBeEmpty();
        favicon.ShouldBe(expectedFavicon);
    }

    [Theory(DisplayName = "Scalar and its favicon should not be mapped when environment is not development")]
    [InlineData("/scalar")]
    [InlineData("/favicon.ico")]
    public async Task GetScalarResource_Should_ReturnNotFound_When_EnvironmentIsNotDevelopment(string path)
    {
        using var response = await Client.GetAsync(path, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Favicon should preserve HTTPS redirection, forwarded headers and API authorization when short circuited")]
    public async Task GetFavicon_Should_PreserveHttpsAndApiAuthorization_When_ShortCircuited()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new FunctionalTestWebApplicationFactory(Environments.Development)
            .WithWebHostBuilder(builder => builder.ConfigureServices(services =>
                services.AddHttpsRedirection(options => options.HttpsPort = 443)));
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var httpResponse = await client.GetAsync("/favicon.ico", cancellationToken);
        client.DefaultRequestHeaders.Add("X-Forwarded-Proto", "https");
        using var forwardedResponse = await client.GetAsync("/favicon.ico", cancellationToken);
        using var protectedResponse = await client.GetAsync("/api/v1/users", cancellationToken);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.TemporaryRedirect);
        httpResponse.Headers.Location.ShouldBe(new Uri("https://localhost/favicon.ico"));
        forwardedResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await forwardedResponse.Content.ReadAsByteArrayAsync(cancellationToken)).ShouldNotBeEmpty();
        protectedResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
