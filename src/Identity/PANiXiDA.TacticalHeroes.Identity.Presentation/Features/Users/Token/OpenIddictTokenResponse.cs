using System.Text.Json.Serialization;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Token;

internal sealed class OpenIddictTokenResponse
{
    [JsonPropertyName("access_token")]
    public required string AccessToken { get; init; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; init; }

    [JsonPropertyName("token_type")]
    public required string TokenType { get; init; }

    [JsonPropertyName("expires_in")]
    public int? ExpiresIn { get; init; }
}
