namespace Organization.Product.Module.Application.Users;

public sealed record UsersFilterParameters(
    string? Role) : FilterParameters;
