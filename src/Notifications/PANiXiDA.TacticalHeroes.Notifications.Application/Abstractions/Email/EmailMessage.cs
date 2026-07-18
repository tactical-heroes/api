namespace PANiXiDA.TacticalHeroes.Notifications.Application.Abstractions.Email;

public sealed record EmailMessage(
    Guid CorrelationId,
    string RecipientEmail,
    string Subject,
    string TextBody,
    string HtmlBody);
