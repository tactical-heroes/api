using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Mvc;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Logout;

public sealed record LogoutRequest(
    [property: FromQuery(Name = "client_id")]
    [property: JsonPropertyName("client_id")]
    string? ClientId,
    [property: FromQuery(Name = "id_token_hint")]
    [property: JsonPropertyName("id_token_hint")]
    string? IdTokenHint,
    [property: FromQuery(Name = "post_logout_redirect_uri")]
    [property: JsonPropertyName("post_logout_redirect_uri")]
    string? PostLogoutRedirectUri,
    [property: FromQuery(Name = "state")]
    [property: JsonPropertyName("state")]
    string? State,
    [property: FromQuery(Name = "ui_locales")]
    [property: JsonPropertyName("ui_locales")]
    string? UiLocales);
