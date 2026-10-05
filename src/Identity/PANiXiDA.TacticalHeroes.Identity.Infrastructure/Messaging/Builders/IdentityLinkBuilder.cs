namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Messaging.Builders;

internal static class IdentityLinkBuilder
{
    public static string Build(
        string template,
        Guid userId,
        string token)
    {
        return template
            .Replace("{userId}", Uri.EscapeDataString(stringToEscape: userId.ToString(format: "D")), StringComparison.Ordinal)
            .Replace("{token}", Uri.EscapeDataString(stringToEscape: token), StringComparison.Ordinal);
    }
}
