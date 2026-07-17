namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Messaging.Options;

public sealed class IdentityMessagingOptions
{
    public const string SectionName = "Identity:Messaging";

    public string AccountConfirmationUrlTemplate { get; init; } =
        "/api/v1/accounts/confirm?accountId={userId}&emailConfirmationToken={token}";

    public string PasswordResetUrlTemplate { get; init; } =
        "/api/v1/accounts/reset-password?accountId={userId}&passwordResetToken={token}";
}
