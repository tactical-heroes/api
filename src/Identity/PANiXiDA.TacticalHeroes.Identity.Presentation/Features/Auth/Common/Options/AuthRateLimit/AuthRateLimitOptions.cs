namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Common.Options.AuthRateLimit;

internal sealed class AuthRateLimitOptions
{
    public const string SectionName = "Identity:RateLimits";

    public AuthEndpointRateLimitOptions Login { get; set; } = new() { PermitLimit = 10 };
    public AuthEndpointRateLimitOptions Register { get; set; } = new() { PermitLimit = 5 };
    public AuthEndpointRateLimitOptions ForgotPassword { get; set; } = new() { PermitLimit = 5 };
    public AuthEndpointRateLimitOptions ResetPassword { get; set; } = new() { PermitLimit = 10 };
    public AuthEndpointRateLimitOptions ResendConfirmationEmail { get; set; } = new() { PermitLimit = 5 };
    public AuthEndpointRateLimitOptions ConfirmEmail { get; set; } = new() { PermitLimit = 10 };
    public AuthEndpointRateLimitOptions ChangePassword { get; set; } = new() { PermitLimit = 5 };
}
