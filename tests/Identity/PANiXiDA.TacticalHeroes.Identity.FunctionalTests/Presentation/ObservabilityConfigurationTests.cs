using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation;

[Collection(FunctionalTestCollectionDefinition.Name)]
public sealed class ObservabilityConfigurationTests(FunctionalTestFixture fixture)
{
    [Fact(DisplayName = "Host should not register OpenTelemetry providers when environment is test")]
    public void CreateHost_Should_NotRegisterTelemetryProviders_When_EnvironmentIsTest()
    {
        var services = fixture.Services;

        services.GetService<TracerProvider>().ShouldBeNull();
        services.GetService<MeterProvider>().ShouldBeNull();
    }

    [Fact(DisplayName = "Host should register OpenTelemetry providers when environment is development")]
    public async Task CreateHost_Should_RegisterTelemetryProviders_When_EnvironmentIsDevelopment()
    {
        await using var factory = new FunctionalTestWebApplicationFactory(Environments.Development);
        using var client = factory.CreateClient();

        var services = factory.Services;

        services.GetService<TracerProvider>().ShouldNotBeNull();
        services.GetService<MeterProvider>().ShouldNotBeNull();
    }
}
