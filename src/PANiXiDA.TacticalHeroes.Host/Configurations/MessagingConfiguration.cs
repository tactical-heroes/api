using PANiXiDA.Core.Infrastructure.Messaging.Wolverine.DependencyInjection;
using PANiXiDA.Core.Infrastructure.Persistence.Ef.Constants;

using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;

using CompendiumApplicationAssembly = PANiXiDA.TacticalHeroes.Compendium.Application.ApplicationAssembly;
using IdentityApplicationAssembly = PANiXiDA.TacticalHeroes.Identity.Application.ApplicationAssembly;
using NotificationsApplicationAssembly = PANiXiDA.TacticalHeroes.Notifications.Application.ApplicationAssembly;
using NotificationsInfrastructure = PANiXiDA.TacticalHeroes.Notifications.Infrastructure.DependencyInjection.ServiceCollectionExtensions;

namespace PANiXiDA.TacticalHeroes.Host.Configurations;

internal static class MessagingConfiguration
{
    internal static WebApplicationBuilder AddMessaging(
        this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString(
            EfConstants.PostgreSqlConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{EfConstants.PostgreSqlConnectionStringName}' was not found.");

        builder.Host.UseWolverineMediator(
            connectionString,
            modules => modules
                .AddModule<IdentityWriteDbContext>(
                    IdentityApplicationAssembly.Instance,
                    typeof(IdentityWriteDbContext).Assembly,
                    NotificationsApplicationAssembly.Instance,
                    typeof(NotificationsInfrastructure).Assembly)
                .AddModule<CompendiumWriteDbContext>(
                    CompendiumApplicationAssembly.Instance,
                    typeof(CompendiumWriteDbContext).Assembly));

        return builder;
    }
}
