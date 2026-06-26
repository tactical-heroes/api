namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Messaging;

internal static class IdentityLinkBuilder
{
    public static string Build(
        string template,
        Guid userId,
        string token)
    {
        return template
            .Replace("{userId}", Uri.EscapeDataString(userId.ToString("D")), StringComparison.Ordinal)
            .Replace("{token}", Uri.EscapeDataString(token), StringComparison.Ordinal);
    }
}
