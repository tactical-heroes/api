using Organization.Product.Module.Domain.Users.Events;

namespace Organization.Product.Module.Application.Users.Events;

public sealed class UserEmailChangedHandler : IEventHandler<UserEmailChanged>
{
    public Task HandleAsync(
        UserEmailChanged @event,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
