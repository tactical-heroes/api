using MailKit.Net.Smtp;

using Microsoft.Extensions.Options;

using MimeKit;

using PANiXiDA.TacticalHeroes.Notifications.Application.Abstractions.Email;
using PANiXiDA.TacticalHeroes.Notifications.Application.Email;
using PANiXiDA.TacticalHeroes.Notifications.Infrastructure.Email.Options.Smtp;

namespace PANiXiDA.TacticalHeroes.Notifications.Infrastructure.Email;

internal sealed class MailKitEmailSender(
    IOptions<SmtpOptions> options) : IEmailSender
{
    public async Task SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken)
    {
        var email = new MimeMessage
        {
            Subject = message.Subject,
            Body = new BodyBuilder
            {
                TextBody = message.TextBody,
                HtmlBody = message.HtmlBody
            }.ToMessageBody()
        };

        email.From.Add(new MailboxAddress(
            options.Value.SenderName,
            options.Value.SenderEmail));
        email.To.Add(MailboxAddress.Parse(message.RecipientEmail));
        email.Headers.Add("X-Correlation-Id", message.CorrelationId.ToString("D"));

        using var smtpClient = new SmtpClient();

        await smtpClient.ConnectAsync(
            host: options.Value.Host,
            port: options.Value.Port,
            options: options.Value.SocketOptions,
            cancellationToken: cancellationToken);

        if (!string.IsNullOrWhiteSpace(options.Value.Username) &&
            !string.IsNullOrWhiteSpace(options.Value.Password))
        {
            await smtpClient.AuthenticateAsync(
                userName: options.Value.Username,
                password: options.Value.Password,
                cancellationToken: cancellationToken);
        }

        await smtpClient.SendAsync(email, cancellationToken);
        await smtpClient.DisconnectAsync(quit: true, cancellationToken);
    }
}
