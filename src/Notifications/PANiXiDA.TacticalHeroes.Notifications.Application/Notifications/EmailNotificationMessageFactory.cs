using PANiXiDA.TacticalHeroes.Notifications.Application.Abstractions.Email;
using PANiXiDA.TacticalHeroes.Notifications.Domain.Notifications.Events;

using System.Globalization;
using System.Net;

namespace PANiXiDA.TacticalHeroes.Notifications.Application.Notifications;

internal static class EmailNotificationMessageFactory
{
    private const string EmailConfirmationSubject = "Confirm your Tactical Heroes email";
    private const string PasswordResetSubject = "Reset your Tactical Heroes password";

    public static EmailMessage Create(
        EmailConfirmationNotificationRequested domainEvent)
    {
        var expiresAtUtc = FormatExpiration(domainEvent.ExpiresAtUtc);
        var encodedUrl = WebUtility.HtmlEncode(domainEvent.ConfirmationUrl);

        return new EmailMessage(
            CorrelationId: domainEvent.IntegrationEventId,
            RecipientEmail: domainEvent.Email,
            Subject: EmailConfirmationSubject,
            TextBody:
                $"Confirm your Tactical Heroes email: {domainEvent.ConfirmationUrl}{Environment.NewLine}" +
                $"This link expires at {expiresAtUtc}.",
            HtmlBody: BuildHtmlBody(
                title: "Confirm your email",
                description: "Confirm your email address to finish setting up your Tactical Heroes account.",
                actionUrl: encodedUrl,
                actionText: "Confirm email",
                expiresAtUtc: expiresAtUtc));
    }

    public static EmailMessage Create(
        PasswordResetNotificationRequested domainEvent)
    {
        var expiresAtUtc = FormatExpiration(domainEvent.ExpiresAtUtc);
        var encodedUrl = WebUtility.HtmlEncode(domainEvent.PasswordResetUrl);

        return new EmailMessage(
            CorrelationId: domainEvent.IntegrationEventId,
            RecipientEmail: domainEvent.Email,
            Subject: PasswordResetSubject,
            TextBody:
                $"Reset your Tactical Heroes password: {domainEvent.PasswordResetUrl}{Environment.NewLine}" +
                $"This link expires at {expiresAtUtc}.",
            HtmlBody: BuildHtmlBody(
                title: "Reset your password",
                description: "Use the button below to choose a new password for your Tactical Heroes account.",
                actionUrl: encodedUrl,
                actionText: "Reset password",
                expiresAtUtc: expiresAtUtc));
    }

    private static string FormatExpiration(DateTimeOffset expiresAtUtc)
    {
        return expiresAtUtc
            .ToUniversalTime()
            .ToString("yyyy-MM-dd HH:mm 'UTC'", CultureInfo.InvariantCulture);
    }

    private static string BuildHtmlBody(
        string title,
        string description,
        string actionUrl,
        string actionText,
        string expiresAtUtc)
    {
        return $$"""
            <!doctype html>
            <html lang="en">
            <head>
              <meta charset="utf-8">
              <meta name="viewport" content="width=device-width, initial-scale=1">
              <title>{{title}}</title>
            </head>
            <body style="margin:0;background:#f4f6f8;font-family:Arial,sans-serif;color:#1f2937;">
              <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="padding:32px 16px;background:#f4f6f8;">
                <tr>
                  <td align="center">
                    <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="max-width:560px;padding:32px;background:#ffffff;border-radius:12px;">
                      <tr><td><h1 style="margin:0 0 16px;font-size:24px;">{{title}}</h1></td></tr>
                      <tr><td><p style="margin:0 0 24px;line-height:1.5;">{{description}}</p></td></tr>
                      <tr><td><a href="{{actionUrl}}" style="display:inline-block;padding:12px 20px;background:#2563eb;color:#ffffff;text-decoration:none;border-radius:8px;font-weight:700;">{{actionText}}</a></td></tr>
                      <tr><td><p style="margin:24px 0 0;color:#6b7280;font-size:14px;">This link expires at {{expiresAtUtc}}.</p></td></tr>
                    </table>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;
    }
}
