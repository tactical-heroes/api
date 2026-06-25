namespace Organization.Product.Module.Application.Users.GetDetails;

public sealed record UserDetailsReadModel(
    Guid Id,
    string Name,
    string Email,
    string Phone,
    DateOnly BirthDate,
    string? Avatar);
