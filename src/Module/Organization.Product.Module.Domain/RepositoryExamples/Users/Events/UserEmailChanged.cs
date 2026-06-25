namespace Organization.Product.Module.Domain.Users.Events;

public sealed record UserEmailChanged(
    Guid UserId,
    string OldEmail,
    string NewEmail) : DomainEvent;
