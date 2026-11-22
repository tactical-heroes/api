using System.Text.Json.Serialization;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Par;

public sealed record ParRequest(
    [property: JsonPropertyName("response_type")] string ResponseType,
    [property: JsonPropertyName("client_id")] string ClientId,
    [property: JsonPropertyName("redirect_uri")] string RedirectUri,
    [property: JsonPropertyName("code_challenge")] string CodeChallenge,
    [property: JsonPropertyName("code_challenge_method")] string CodeChallengeMethod,
    [property: JsonPropertyName("scope")] string? Scope = null,
    [property: JsonPropertyName("state")] string? State = null,
    [property: JsonPropertyName("response_mode")] string? ResponseMode = null,
    [property: JsonPropertyName("nonce")] string? Nonce = null,
    [property: JsonPropertyName("display")] string? Display = null,
    [property: JsonPropertyName("prompt")] string? Prompt = null,
    [property: JsonPropertyName("max_age")] long? MaxAge = null,
    [property: JsonPropertyName("ui_locales")] string? UiLocales = null,
    [property: JsonPropertyName("id_token_hint")] string? IdTokenHint = null,
    [property: JsonPropertyName("login_hint")] string? LoginHint = null,
    [property: JsonPropertyName("acr_values")] string? AcrValues = null,
    [property: JsonPropertyName("claims")] string? Claims = null,
    [property: JsonPropertyName("claims_locales")] string? ClaimsLocales = null);
