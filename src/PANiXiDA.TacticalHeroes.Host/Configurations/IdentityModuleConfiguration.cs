using PANiXiDA.TacticalHeroes.Identity.Infrastructure.DependencyInjection;
using PANiXiDA.TacticalHeroes.Identity.Presentation.DependencyInjection;

namespace PANiXiDA.TacticalHeroes.Host.Configurations;

internal static class IdentityModuleConfiguration
{
    internal static WebApplicationBuilder AddIdentityModule(
        this WebApplicationBuilder builder)
    {
        builder.Services.AddInfrastructure(
            builder.Configuration,
            builder.Environment);
        builder.Services.AddPresentation(builder.Configuration);
        builder.Host.UseInfrastructure(builder.Configuration);

        return builder;
    }
}
