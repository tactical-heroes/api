using OpenIddict.Validation.SystemNetHttp;

using PANiXiDA.Core.Presentation.Http.DependencyInjection;

using PANiXiDA.TacticalHeroes.Host.Common;
using PANiXiDA.TacticalHeroes.Host.Configurations.Authentication;

using static OpenIddict.Validation.OpenIddictValidationEvents;

using CompendiumPresentationAssembly = PANiXiDA.TacticalHeroes.Compendium.Presentation.PresentationAssembly;
using FileManagerPresentationAssembly = PANiXiDA.TacticalHeroes.FileManager.Presentation.PresentationAssembly;
using HttpServiceCollectionExtensions = PANiXiDA.Core.Presentation.Http.DependencyInjection.ServiceCollectionExtensions;
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
            CompendiumPresentationAssembly.Instance,
            FileManagerPresentationAssembly.Instance);

        builder.Services.AddOpenIddict().AddValidation(options =>
        {
            var descriptor = OpenIddictValidationSystemNetHttpHandlers.SendHttpRequest<ApplyIntrospectionRequestContext>.Descriptor;
            options.RemoveEventHandler(descriptor);
            options.AddEventHandler<ApplyIntrospectionRequestContext>(handler => handler
                .AddFilter<OpenIddictValidationSystemNetHttpHandlerFilters.RequireHttpUri>()
                .UseSingletonHandler<SendIntrospectionHttpRequestHandler>()
                .SetOrder(descriptor.Order));
        });

        return builder;
    }

    internal static WebApplication UseHttp(this WebApplication app)
    {
        HttpServiceCollectionExtensions.UseHttp(app);

        return app;
    }
}
