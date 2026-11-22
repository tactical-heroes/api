using PANiXiDA.Core.Presentation.Http.DependencyInjection;

using PANiXiDA.TacticalHeroes.Host.Common;

using BattlePresentationAssembly = PANiXiDA.TacticalHeroes.Modules.Battle.Presentation.PresentationAssembly;
using BuildsPresentationAssembly = PANiXiDA.TacticalHeroes.Modules.Builds.Presentation.PresentationAssembly;
using CompendiumPresentationAssembly = PANiXiDA.TacticalHeroes.Modules.Compendium.Presentation.PresentationAssembly;
using FileManagerPresentationAssembly = PANiXiDA.TacticalHeroes.Modules.FileManager.Presentation.PresentationAssembly;
using GamesPresentationAssembly = PANiXiDA.TacticalHeroes.Modules.Games.Presentation.PresentationAssembly;
using HttpServiceCollectionExtensions = PANiXiDA.Core.Presentation.Http.DependencyInjection.ServiceCollectionExtensions;
using IdentityPresentationAssembly = PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.PresentationAssembly;
using MatchmakingPresentationAssembly = PANiXiDA.TacticalHeroes.Modules.Matchmaking.Presentation.PresentationAssembly;
using PlayersPresentationAssembly = PANiXiDA.TacticalHeroes.Modules.Players.Presentation.PresentationAssembly;

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
            BattlePresentationAssembly.Instance,
            MatchmakingPresentationAssembly.Instance,
            GamesPresentationAssembly.Instance,
            BuildsPresentationAssembly.Instance,
            PlayersPresentationAssembly.Instance,
            FileManagerPresentationAssembly.Instance);

        return builder;
    }

    internal static WebApplication UseHttp(this WebApplication app)
    {
        HttpServiceCollectionExtensions.UseHttp(app);

        return app;
    }
}
