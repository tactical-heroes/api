using System.Text.Json.Serialization;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Revoke;

public sealed record RevokeRequest(
    [property: JsonPropertyName("client_id")] string ClientId,
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("token_type_hint")] string? TokenTypeHint);
