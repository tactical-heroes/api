using PANiXiDA.Core.Presentation.SignalR.DependencyInjection;

using PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchSearches.Streaming.State;

using PolyType;

using MatchmakingPresentationAssembly = PANiXiDA.TacticalHeroes.Matchmaking.Presentation.PresentationAssembly;

namespace PANiXiDA.TacticalHeroes.Host.Configurations;

internal static partial class SignalRConfiguration
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

    [GenerateShapeFor<MatchSearchStateResponse>]
    private sealed partial class SignalRMessagePackTypes;
}
