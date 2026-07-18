using System.Text.Json.Serialization;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Introspect;

public sealed record IntrospectErrorResponse(
    [property: JsonPropertyName("error")] string Error,
    [property: JsonPropertyName("error_description")] string? ErrorDescription);
