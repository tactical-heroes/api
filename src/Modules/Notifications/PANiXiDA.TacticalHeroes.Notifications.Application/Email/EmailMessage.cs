namespace PANiXiDA.TacticalHeroes.Modules.Notifications.Application.Email;

public sealed record EmailMessage(
    Guid CorrelationId,
    string RecipientEmail,
    string Subject,
    string TextBody,
    string HtmlBody);
