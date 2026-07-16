using System.Text.Json.Serialization;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Par;

public sealed record ParRequest(
    [property: JsonPropertyName("response_type")] string ResponseType,
    [property: JsonPropertyName("client_id")] string ClientId,
    [property: JsonPropertyName("redirect_uri")] string RedirectUri,
    [property: JsonPropertyName("scope")] string? Scope,
    [property: JsonPropertyName("state")] string? State,
    [property: JsonPropertyName("code_challenge")] string CodeChallenge,
    [property: JsonPropertyName("code_challenge_method")] string CodeChallengeMethod,
    [property: JsonPropertyName("response_mode")] string? ResponseMode,
    [property: JsonPropertyName("nonce")] string? Nonce,
    [property: JsonPropertyName("display")] string? Display,
    [property: JsonPropertyName("prompt")] string? Prompt,
    [property: JsonPropertyName("max_age")] long? MaxAge,
    [property: JsonPropertyName("ui_locales")] string? UiLocales,
    [property: JsonPropertyName("id_token_hint")] string? IdTokenHint,
    [property: JsonPropertyName("login_hint")] string? LoginHint,
    [property: JsonPropertyName("acr_values")] string? AcrValues,
    [property: JsonPropertyName("claims")] string? Claims,
    [property: JsonPropertyName("claims_locales")] string? ClaimsLocales);
