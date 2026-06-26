using System.Text.Json.Serialization;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Token;

internal sealed class OpenIddictTokenRequest
{
    [JsonPropertyName("client_id")]
    public required string ClientId { get; init; }

    [JsonPropertyName("grant_type")]
    public required string GrantType { get; init; }

    public string? Username { get; init; }

    public string? Password { get; init; }

    public string? Scope { get; init; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; init; }
}
