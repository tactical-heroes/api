namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Messaging.Options.IdentityMessaging;

public sealed class IdentityMessagingOptions
{
    public const string SectionName = "Identity:Messaging";

    public string EmailConfirmationUrlTemplate { get; init; } = string.Empty;

    public string PasswordResetUrlTemplate { get; init; } = string.Empty;
}
