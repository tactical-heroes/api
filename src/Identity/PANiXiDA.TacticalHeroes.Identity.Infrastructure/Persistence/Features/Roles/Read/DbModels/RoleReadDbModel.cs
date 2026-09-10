using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read.DbModels;

public sealed class RoleReadDbModel : ReadDbModel<Guid>
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? Name { get; set; }
    public string? NormalizedName { get; set; }
    public string? ConcurrencyStamp { get; set; }

    public ICollection<UserRoleReadDbModel> Users { get; set; } = [];
    public ICollection<RoleClaimReadDbModel> Claims { get; set; } = [];
}
