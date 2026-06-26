using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Messaging;

internal static class IdentityMessagingServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityMessaging(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.Configure<IdentityMessagingOptions>(
            configuration.GetSection(IdentityMessagingOptions.SectionName));

        return serviceCollection;
    }
}
