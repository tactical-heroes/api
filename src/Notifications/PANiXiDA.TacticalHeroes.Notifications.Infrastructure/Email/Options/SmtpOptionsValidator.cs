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

        if (string.IsNullOrWhiteSpace(value: options.Host))
        {
            failures.Add(item: $"{SmtpOptions.SectionName}:Host must not be empty.");
        }

        if (options.Port is <= 0 or > ushort.MaxValue)
        {
            failures.Add(item: $"{SmtpOptions.SectionName}:Port must be between 1 and {ushort.MaxValue}.");
        }

        if (!MailboxAddress.TryParse(text: options.SenderEmail, mailbox: out _))
        {
            failures.Add(item: $"{SmtpOptions.SectionName}:SenderEmail must be a valid email address.");
        }

        if (string.IsNullOrWhiteSpace(value: options.SenderName))
        {
            failures.Add(item: $"{SmtpOptions.SectionName}:SenderName must not be empty.");
        }

        var hasUsername = !string.IsNullOrWhiteSpace(value: options.Username);
        var hasPassword = !string.IsNullOrWhiteSpace(value: options.Password);

        if (hasUsername != hasPassword)
        {
            failures.Add(
                item: $"{SmtpOptions.SectionName}:Username and Password must either both be set or both be empty.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures: failures);
    }
}
