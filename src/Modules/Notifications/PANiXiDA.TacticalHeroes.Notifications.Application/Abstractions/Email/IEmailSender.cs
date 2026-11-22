using PANiXiDA.TacticalHeroes.Notifications.Application.Email;

namespace PANiXiDA.TacticalHeroes.Notifications.Application.Abstractions.Email;

public interface IEmailSender
{
    Task SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken);
}
