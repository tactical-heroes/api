using System.Text.Json.Serialization;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Features.OAuth.ExchangeToken;

public sealed record ExchangeTokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("token_type")] string TokenType,
    [property: JsonPropertyName("expires_in")] long? ExpiresIn,
    [property: JsonPropertyName("refresh_token")] string? RefreshToken,
    [property: JsonPropertyName("issued_token_type")] string? IssuedTokenType,
    [property: JsonPropertyName("id_token")] string? IdToken);
