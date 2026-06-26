using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using PANiXiDA.Core.Application.Messaging.EventBus;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Seeding;

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
            var seederHostedService = services.SingleOrDefault(descriptor =>
                descriptor.ServiceType == typeof(IHostedService) &&
                descriptor.ImplementationType == typeof(IdentityProviderApplicationSeederHostedService));

            if (seederHostedService is not null)
            {
                services.Remove(seederHostedService);
            }

            services.RemoveAll<IEventBus>();
            services.AddSingleton(EventBus);
            services.AddSingleton<IEventBus>(serviceProvider =>
                serviceProvider.GetRequiredService<CapturingEventBus>());
            services.RunWolverineInSoloMode();
        });
    }
}
