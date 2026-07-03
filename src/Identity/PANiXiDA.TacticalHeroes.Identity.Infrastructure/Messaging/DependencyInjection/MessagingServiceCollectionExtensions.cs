using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Messaging.Options;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Messaging.DependencyInjection;

internal static class MessagingServiceCollectionExtensions
{
    public static IServiceCollection AddMessaging(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.Configure<IdentityMessagingOptions>(
            configuration.GetSection(IdentityMessagingOptions.SectionName));

        serviceCollection.AddWolverineMediator<IdentityWriteDbContext>();

        return serviceCollection;
    }
}
