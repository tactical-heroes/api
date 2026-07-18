using Microsoft.Extensions.Options;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth;

internal sealed class OAuthTokenOptionsValidator
    : IValidateOptions<OAuthTokenOptions>
{
    public ValidateOptionsResult Validate(
        string? name,
        OAuthTokenOptions options)
    {
        return string.IsNullOrWhiteSpace(options.Audience)
            ? ValidateOptionsResult.Fail(
                failureMessage: $"{OAuthTokenOptions.SectionName}:Audience must not be empty.")
            : ValidateOptionsResult.Success;
    }
}
