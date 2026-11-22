using PANiXiDA.TacticalHeroes.Modules.Notifications.Application.Email;

namespace PANiXiDA.TacticalHeroes.Modules.Notifications.Application.Abstractions.Email;

public interface IEmailSender
{
    Task SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken);
}
