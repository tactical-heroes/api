using System.Text.Json.Serialization;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.ExchangeToken;

public sealed record ExchangeTokenRequest(
    [property: JsonPropertyName("grant_type")] string GrantType,
    [property: JsonPropertyName("client_id")] string ClientId,
    [property: JsonPropertyName("client_secret")] string? ClientSecret,
    [property: JsonPropertyName("code")] string? Code,
    [property: JsonPropertyName("code_verifier")] string? CodeVerifier,
    [property: JsonPropertyName("redirect_uri")] string? RedirectUri,
    [property: JsonPropertyName("refresh_token")] string? RefreshToken,
    [property: JsonPropertyName("scope")] string? Scope,
    [property: JsonPropertyName("subject_token")] string? SubjectToken,
    [property: JsonPropertyName("subject_token_type")] string? SubjectTokenType,
    [property: JsonPropertyName("requested_token_type")] string? RequestedTokenType,
    [property: JsonPropertyName("audience")] string? Audience,
    [property: JsonPropertyName("resource")] string? Resource);
