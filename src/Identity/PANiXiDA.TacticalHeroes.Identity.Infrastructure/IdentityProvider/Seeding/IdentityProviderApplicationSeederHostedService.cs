using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Seeding;

internal sealed class IdentityProviderApplicationSeederHostedService(
    IServiceScopeFactory serviceScopeFactory)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IdentityProviderApplicationSeeder>();

        await seeder.SeedAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
