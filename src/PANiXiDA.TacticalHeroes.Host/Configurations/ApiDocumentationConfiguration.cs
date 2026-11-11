using System.Text.Json.Serialization;

using PANiXiDA.Core.Presentation.Scalar.DependencyInjection;

using PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchSearches.Streaming.State;

namespace PANiXiDA.TacticalHeroes.Host.Configurations;

internal static partial class ApiDocumentationConfiguration
{
    internal static WebApplicationBuilder AddApiDocumentation(this WebApplicationBuilder builder)
    {
        builder.Services.AddApiDocumentation(builder.Configuration, ApiDocumentationJsonContext.Default);

        return builder;
    }

    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(MatchSearchStateResponse))]
    private sealed partial class ApiDocumentationJsonContext : JsonSerializerContext;
}
