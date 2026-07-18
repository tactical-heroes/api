using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.Clients;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.Lockout;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.Password;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.TokenProviders;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.User;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.DependencyInjection;

internal static class IdentityProviderOptionsServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityProviderOptionsValidators(
        this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<
            IValidateOptions<IdentityProviderOptions>,
            IdentityProviderOptionsValidator>();
        serviceCollection.AddSingleton<
            IValidateOptions<IdentityProviderOptions>,
            IdentityProviderUserOptionsValidator>();
        serviceCollection.AddSingleton<
            IValidateOptions<IdentityProviderOptions>,
            IdentityProviderPasswordOptionsValidator>();
        serviceCollection.AddSingleton<
            IValidateOptions<IdentityProviderOptions>,
            IdentityProviderLockoutOptionsValidator>();
        serviceCollection.AddSingleton<
            IValidateOptions<IdentityProviderOptions>,
            IdentityProviderTokenProviderOptionsValidator>();
        serviceCollection.AddSingleton<
            IValidateOptions<IdentityProviderOptions>,
            IdentityProviderClientsOptionsValidator>();

        return serviceCollection;
    }
}
