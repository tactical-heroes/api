using Microsoft.Extensions.Options;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Common.Options.AuthRateLimit;

internal sealed class AuthRateLimitOptionsValidator : IValidateOptions<AuthRateLimitOptions>
{
    public ValidateOptionsResult Validate(string? name, AuthRateLimitOptions options)
    {
        List<string> failures = [];
        Validate(options.Login, nameof(options.Login));
        Validate(options.Register, nameof(options.Register));
        Validate(options.ForgotPassword, nameof(options.ForgotPassword));
        Validate(options.ResetPassword, nameof(options.ResetPassword));
        Validate(options.ResendConfirmationEmail, nameof(options.ResendConfirmationEmail));
        Validate(options.ConfirmEmail, nameof(options.ConfirmEmail));
        Validate(options.ChangePassword, nameof(options.ChangePassword));

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);

        void Validate(AuthEndpointRateLimitOptions policy, string policyName)
        {
            if (policy.PermitLimit <= 0 || policy.Window <= TimeSpan.Zero)
            {
                failures.Add(
                    $"{AuthRateLimitOptions.SectionName}:{policyName} requires a positive PermitLimit and Window.");
            }
        }
    }
}
