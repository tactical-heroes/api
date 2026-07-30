using Microsoft.Extensions.Options;

using MimeKit;

namespace PANiXiDA.TacticalHeroes.Notifications.Infrastructure.Email.Options;

internal sealed class SmtpOptionsValidator : IValidateOptions<SmtpOptions>
{
    public ValidateOptionsResult Validate(
        string? name,
        SmtpOptions options)
    {
        List<string> failures = [];

        if (string.IsNullOrWhiteSpace(options.Host))
        {
            failures.Add($"{SmtpOptions.SectionName}:Host must not be empty.");
        }

        if (options.Port is <= 0 or > ushort.MaxValue)
        {
            failures.Add($"{SmtpOptions.SectionName}:Port must be between 1 and {ushort.MaxValue}.");
        }

        if (!MailboxAddress.TryParse(options.SenderEmail, out _))
        {
            failures.Add($"{SmtpOptions.SectionName}:SenderEmail must be a valid email address.");
        }

        if (string.IsNullOrWhiteSpace(options.SenderName))
        {
            failures.Add($"{SmtpOptions.SectionName}:SenderName must not be empty.");
        }

        var hasUsername = !string.IsNullOrWhiteSpace(options.Username);
        var hasPassword = !string.IsNullOrWhiteSpace(options.Password);

        if (hasUsername != hasPassword)
        {
            failures.Add(
                $"{SmtpOptions.SectionName}:Username and Password must either both be set or both be empty.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
