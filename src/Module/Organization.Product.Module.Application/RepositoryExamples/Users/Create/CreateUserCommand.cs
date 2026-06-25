namespace Organization.Product.Module.Application.Users.Create;

public sealed record CreateUserCommand(
    string Role,
    string Name,
    string Email,
    string Phone,
    DateOnly BirthDate,
    string? Avatar) : ICommand<Result<Guid>>;
