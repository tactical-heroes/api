namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Messaging.Options;

public sealed class IdentityMessagingOptions
{
    public const string SectionName = "Identity:Messaging";

    public string AccountConfirmationUrlTemplate { get; init; } =
        "/api/v1/identity/auth/confirm?userId={userId}&token={token}";

    public string PasswordResetUrlTemplate { get; init; } =
        "/api/v1/identity/auth/password-reset/confirm?userId={userId}&token={token}";
}
