namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Options.OAuthToken;

public sealed class OAuthTokenOptions
{
    public const string SectionName = "Identity:Provider";

    public string Audience { get; init; } = string.Empty;
}
