using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentation(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.AddHttp(configuration);
        serviceCollection.Configure<OAuthSpaOptions>(
            configuration.GetSection(OAuthSpaOptions.SectionName));
        serviceCollection.Configure<OAuthTokenOptions>(
            configuration.GetSection(OAuthTokenOptions.SectionName));
        return serviceCollection;
    }
}
