using PANiXiDA.TacticalHeroes.Identity.Infrastructure.DependencyInjection;
using PANiXiDA.TacticalHeroes.Identity.Presentation.DependencyInjection;

namespace PANiXiDA.TacticalHeroes.Host.Configurations;

internal static class IdentityModuleConfiguration
{
    internal static WebApplicationBuilder AddIdentityModule(
        this WebApplicationBuilder builder)
    {
        builder.Services.AddInfrastructure(
            configuration: builder.Configuration,
            environment: builder.Environment);
        builder.Services.AddPresentation(configuration: builder.Configuration);
        builder.Host.UseInfrastructure();

        return builder;
    }
}
