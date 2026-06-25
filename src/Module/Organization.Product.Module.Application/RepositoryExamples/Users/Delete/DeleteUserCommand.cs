namespace Organization.Product.Module.Application.Users.Delete;

public sealed record DeleteUserCommand(Guid Id)
    : ICommand<Result>;
