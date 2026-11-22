using System.Text.Json.Serialization;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.GetUserInfo;

public sealed record GetUserInfoResponse(
    [property: JsonPropertyName("sub")] string Subject,
    [property: JsonPropertyName("name")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Name,
    [property: JsonPropertyName("email")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Email,
    [property: JsonPropertyName("email_verified")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    bool? EmailVerified,
    [property: JsonPropertyName("role")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyCollection<string>? Role);
