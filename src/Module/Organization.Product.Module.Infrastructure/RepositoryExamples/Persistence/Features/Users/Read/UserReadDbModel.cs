namespace Organization.Product.Module.Infrastructure.Persistence.Features.Users.Read;

public sealed class UserReadDbModel : AuditableReadDbModel<Guid>
{
    public string Role { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public string? Avatar { get; set; }
}
