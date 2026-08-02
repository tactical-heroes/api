namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Messaging.Options.IdentityMessaging;

public sealed class IdentityMessagingOptions
{
    public const string SectionName = "Identity:Messaging";

    public string EmailConfirmationUrlTemplate { get; init; } =
        "/api/v1/auth/confirm-email?userId={userId}&emailConfirmationToken={token}";

    public string PasswordResetUrlTemplate { get; init; } =
        "/api/v1/auth/reset-password?userId={userId}&passwordResetToken={token}";
}
