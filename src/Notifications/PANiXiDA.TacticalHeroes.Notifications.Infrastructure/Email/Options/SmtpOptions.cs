using MailKit.Security;

namespace PANiXiDA.TacticalHeroes.Notifications.Infrastructure.Email.Options;

public sealed class SmtpOptions
{
    public const string SectionName = "Notifications:Email:Smtp";

    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 1025;
    public SecureSocketOptions SocketOptions { get; init; } = SecureSocketOptions.None;
    public string? Username { get; init; }
    public string? Password { get; init; }
    public string SenderEmail { get; init; } = "no-reply@tactical-heroes.local";
    public string SenderName { get; init; } = "Tactical Heroes";
}
