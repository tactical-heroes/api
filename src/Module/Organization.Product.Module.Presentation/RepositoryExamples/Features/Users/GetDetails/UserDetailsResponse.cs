namespace Organization.Product.Module.Presentation.Features.Users.GetDetails;

internal sealed record UserDetailsResponse(
    Guid Id,
    string Name,
    string Email,
    string Phone,
    DateOnly BirthDate,
    string? Avatar);
