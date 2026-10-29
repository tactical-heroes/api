using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation.Matching;

using Wolverine;

namespace PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation;

internal sealed class FunctionalTestWebApplicationFactory(bool matchmakingScheduleEnabled)
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["OpenIddictValidationOptions:ClientSecret"] = "functional-test-introspection-secret",
                ["Identity:Provider:Clients:1:ClientSecret"] = "functional-test-introspection-secret",
                ["FileManager:Schedules:ReconcileFileUploadsCommand:Enabled"] = "false",
                ["Matchmaking:Schedules:ProcessMatchmakingCommand:Enabled"] = matchmakingScheduleEnabled.ToString()
            }));
        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.ConfigureServices(services =>
        {
            var seederHostedServices = services
                .Where(descriptor =>
                    descriptor.ServiceType == typeof(IHostedService)
                        && descriptor.ImplementationType?.Name
                            == "IdentityProviderApplicationSeederHostedService")
                .ToArray();

            foreach (var seederHostedService in seederHostedServices)
            {
                services.Remove(seederHostedService);
            }

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthenticationHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthenticationHandler.SchemeName;
            })
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    TestAuthenticationHandler.SchemeName, _ => { });
            services.RunWolverineInSoloMode();
            services.AddSingleton<MatchFoundProbe>();
            services.AddSingleton<ConcurrentMatchSaveBarrier>();
            services.AddSingleton<IInterceptor>(provider => provider.GetRequiredService<ConcurrentMatchSaveBarrier>());
            services.ConfigureWolverine(options =>
            {
                options.CodeGeneration.SourceCodeWritingEnabled = false;
                options.Discovery.IncludeType(typeof(MatchFoundProbeHandler));
            });
        });
    }
}
