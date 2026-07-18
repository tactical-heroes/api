using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Messaging.Options;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Messaging.DependencyInjection;

internal static class MessagingServiceCollectionExtensions
{
    public static IServiceCollection AddMessaging(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.AddSingleton<
            IValidateOptions<IdentityMessagingOptions>,
            IdentityMessagingOptionsValidator>();
        serviceCollection
            .AddOptions<IdentityMessagingOptions>()
            .Bind(configuration.GetSection(IdentityMessagingOptions.SectionName))
            .ValidateOnStart();

        serviceCollection.AddWolverineMediator<IdentityWriteDbContext>();

        return serviceCollection;
    }
}
