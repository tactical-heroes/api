using System.Text.Json.Serialization;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Revoke;

public sealed record RevokeErrorResponse(
    [property: JsonPropertyName("error")] string Error,
    [property: JsonPropertyName("error_description")] string? ErrorDescription);
