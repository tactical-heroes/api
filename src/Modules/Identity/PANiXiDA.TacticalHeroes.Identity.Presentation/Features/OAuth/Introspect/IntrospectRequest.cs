using System.Text.Json.Serialization;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Introspect;

public sealed record IntrospectRequest(
    [property: JsonPropertyName("client_id")] string ClientId,
    [property: JsonPropertyName("client_secret")] string? ClientSecret,
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("token_type_hint")] string? TokenTypeHint);
