namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth;

public sealed class OAuthTokenOptions
{
    public const string SectionName = "Identity:Provider";

    public string Audience { get; init; } = string.Empty;
}
