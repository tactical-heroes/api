using PANiXiDA.Core.Infrastructure.Messaging.Wolverine.DependencyInjection;
using PANiXiDA.Core.Infrastructure.Persistence.Ef.Constants;

using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;

using CompendiumApplicationAssembly = PANiXiDA.TacticalHeroes.Compendium.Application.ApplicationAssembly;
using IdentityApplicationAssembly = PANiXiDA.TacticalHeroes.Identity.Application.ApplicationAssembly;

namespace PANiXiDA.TacticalHeroes.Host.Configurations;

internal static class MessagingConfiguration
{
    internal static WebApplicationBuilder AddMessaging(
        this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString(
            name: EfConstants.PostgreSqlConnectionStringName)
            ?? throw new InvalidOperationException(
                message: $"Connection string '{EfConstants.PostgreSqlConnectionStringName}' was not found.");

        builder.Host.UseWolverineMediator(
            messageStoreConnectionString: connectionString,
            configureModules: modules => modules
                .AddModule<IdentityWriteDbContext>(
                    requestAssembly: IdentityApplicationAssembly.Instance,
                    handlerAssemblies: typeof(IdentityWriteDbContext).Assembly)
                .AddModule<CompendiumWriteDbContext>(
                    requestAssembly: CompendiumApplicationAssembly.Instance,
                    handlerAssemblies: typeof(CompendiumWriteDbContext).Assembly));

        return builder;
    }
}
