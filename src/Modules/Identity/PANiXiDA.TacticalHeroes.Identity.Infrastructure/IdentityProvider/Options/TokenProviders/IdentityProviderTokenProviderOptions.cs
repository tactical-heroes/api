namespace PANiXiDA.TacticalHeroes.Modules.Identity.Infrastructure.IdentityProvider.Options.TokenProviders;

public sealed class IdentityProviderTokenProviderOptions
{
    public string EmailConfirmation { get; init; } = "email_confirmation";

    public string PasswordReset { get; init; } = "password_reset";
}
