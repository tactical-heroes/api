using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Wolverine;

namespace PANiXiDA.TacticalHeroes.Players.FunctionalTests.Presentation;

internal sealed class FunctionalTestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        const string MatchmakingScheduleVariable = "Matchmaking__Schedules__ProcessMatchmakingCommand__Enabled";
        var previousMatchmakingSchedule = Environment.GetEnvironmentVariable(MatchmakingScheduleVariable);
        const string Variable = "Matchmaking__Schedules__ExpireMatchSearchesCommand__Enabled";
        var previous = Environment.GetEnvironmentVariable(Variable);
        const string FileManagerScheduleVariable = "FileManager__Schedules__ReconcileFileUploadsCommand__Enabled";
        var previousFileManagerSchedule = Environment.GetEnvironmentVariable(FileManagerScheduleVariable);
        try
        {
            Environment.SetEnvironmentVariable(MatchmakingScheduleVariable, "false");
            Environment.SetEnvironmentVariable(Variable, "false");
            Environment.SetEnvironmentVariable(FileManagerScheduleVariable, "false");
            return base.CreateHost(builder);
        }
        finally
        {
            Environment.SetEnvironmentVariable(MatchmakingScheduleVariable, previousMatchmakingSchedule);
            Environment.SetEnvironmentVariable(Variable, previous);
            Environment.SetEnvironmentVariable(FileManagerScheduleVariable, previousFileManagerSchedule);
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["OpenIddictValidationOptions:ClientSecret"] = "functional-test-introspection-secret",
                ["Identity:Provider:Clients:1:ClientSecret"] = "functional-test-introspection-secret"
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

            services.RunWolverineInSoloMode();
        });
    }
}
