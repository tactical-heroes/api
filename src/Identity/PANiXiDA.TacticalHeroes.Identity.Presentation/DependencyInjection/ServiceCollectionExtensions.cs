using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentation(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.AddHttp(configuration);
        serviceCollection.AddSingleton<IValidateOptions<OAuthSpaOptions>, OAuthSpaOptionsValidator>();
        serviceCollection.AddSingleton<IValidateOptions<OAuthTokenOptions>, OAuthTokenOptionsValidator>();
        serviceCollection
            .AddOptions<OAuthSpaOptions>()
            .Bind(configuration.GetSection(OAuthSpaOptions.SectionName))
            .ValidateOnStart();
        serviceCollection
            .AddOptions<OAuthTokenOptions>()
            .Bind(configuration.GetSection(OAuthTokenOptions.SectionName))
            .ValidateOnStart();

        return serviceCollection;
    }
}
