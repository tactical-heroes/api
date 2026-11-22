using System.Text.Json.Serialization;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Introspect;

public sealed record IntrospectResponse(
    [property: JsonPropertyName("active")] bool Active,
    [property: JsonPropertyName("scope")] string? Scope,
    [property: JsonPropertyName("client_id")] string? ClientId,
    [property: JsonPropertyName("username")] string? UserName,
    [property: JsonPropertyName("token_type")] string? TokenType,
    [property: JsonPropertyName("exp")] long? ExpiresAt,
    [property: JsonPropertyName("iat")] long? IssuedAt,
    [property: JsonPropertyName("nbf")] long? NotBefore,
    [property: JsonPropertyName("sub")] string? Subject,
    [property: JsonPropertyName("aud")] string? Audience,
    [property: JsonPropertyName("iss")] string? Issuer,
    [property: JsonPropertyName("jti")] string? TokenId);
