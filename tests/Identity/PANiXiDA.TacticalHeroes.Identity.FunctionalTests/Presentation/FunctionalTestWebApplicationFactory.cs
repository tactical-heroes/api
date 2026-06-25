using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using PANiXiDA.Core.Application.Messaging.EventBus;

using Wolverine;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation;

internal sealed class FunctionalTestWebApplicationFactory
    : WebApplicationFactory<Program>
{
    public CapturingEventBus EventBus { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IEventBus>();
            services.AddSingleton(EventBus);
            services.AddSingleton<IEventBus>(serviceProvider =>
                serviceProvider.GetRequiredService<CapturingEventBus>());
            services.RunWolverineInSoloMode();
        });
    }
}
