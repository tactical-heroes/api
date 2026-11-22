using System.Text.Json.Serialization;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Features.OAuth.Par;

public sealed record ParResponse(
    [property: JsonPropertyName("request_uri")] string RequestUri,
    [property: JsonPropertyName("expires_in")] long ExpiresIn);
