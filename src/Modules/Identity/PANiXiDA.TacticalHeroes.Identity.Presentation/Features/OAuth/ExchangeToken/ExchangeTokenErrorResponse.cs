using System.Text.Json.Serialization;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Features.OAuth.ExchangeToken;

public sealed record ExchangeTokenErrorResponse(
    [property: JsonPropertyName("error")] string Error,
    [property: JsonPropertyName("error_description")] string? ErrorDescription);
