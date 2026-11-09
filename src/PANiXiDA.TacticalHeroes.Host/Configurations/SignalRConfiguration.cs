using PANiXiDA.Core.Presentation.SignalR.DependencyInjection;

using MatchmakingPresentationAssembly = PANiXiDA.TacticalHeroes.Matchmaking.Presentation.PresentationAssembly;

namespace PANiXiDA.TacticalHeroes.Host.Configurations;

internal static class SignalRConfiguration
{
    internal static WebApplicationBuilder AddSignalR(this WebApplicationBuilder builder)
    {
        builder.Services.AddSignalR(builder.Configuration, SignalRMessagePackTypes.GeneratedTypeShapeProvider);

        return builder;
    }

    internal static WebApplication UseSignalR(this WebApplication app)
    {
        return app.UseSignalR(MatchmakingPresentationAssembly.Instance);
    }
}
