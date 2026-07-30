namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Messaging.Builders;

internal static class IdentityLinkBuilder
{
    public static string Build(
        string template,
        Guid userId,
        string token)
    {
        return template
            .Replace(oldValue: "{userId}", newValue: Uri.EscapeDataString(stringToEscape: userId.ToString(format: "D")), comparisonType: StringComparison.Ordinal)
            .Replace(oldValue: "{token}", newValue: Uri.EscapeDataString(stringToEscape: token), comparisonType: StringComparison.Ordinal);
    }
}
