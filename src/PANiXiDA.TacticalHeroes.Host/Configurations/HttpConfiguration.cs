using PANiXiDA.Core.Presentation.Http.DependencyInjection;

using PANiXiDA.TacticalHeroes.Host.Common;

using CompendiumPresentationAssembly = PANiXiDA.TacticalHeroes.Compendium.Presentation.PresentationAssembly;
using IdentityPresentationAssembly = PANiXiDA.TacticalHeroes.Identity.Presentation.PresentationAssembly;

namespace PANiXiDA.TacticalHeroes.Host.Configurations;

internal static class HttpConfiguration
{
    internal static WebApplicationBuilder AddHttp(
        this WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Limits.MaxRequestBodySize = FilesConstants.FileRequestSizeLimit;
        });

        builder.Services.AddHttp(
            builder.Configuration,
            IdentityPresentationAssembly.Instance,
            CompendiumPresentationAssembly.Instance);

        return builder;
    }

    internal static WebApplication UseHttp(this WebApplication app)
    {
        ServiceCollectionExtensions.UseHttp(app);

        return app;
    }
}
