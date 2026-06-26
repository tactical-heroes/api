using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Messaging.Options;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Messaging.DependencyInjection;

internal static class MessagingServiceCollectionExtensions
{
    public static IServiceCollection AddMessaging(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.Configure<IdentityMessagingOptions>(
            configuration.GetSection(IdentityMessagingOptions.SectionName));

        return serviceCollection;
    }
}
