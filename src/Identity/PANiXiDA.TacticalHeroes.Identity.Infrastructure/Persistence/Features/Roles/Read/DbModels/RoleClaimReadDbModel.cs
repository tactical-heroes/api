using System.ComponentModel.DataAnnotations;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read.DbModels;

public sealed class RoleClaimReadDbModel : ReadDbModel<int>
{
    public Guid RoleId { get; set; }

    [MaxLength(256)]
    public string ClaimType { get; set; } = string.Empty;

    [MaxLength(1024)]
    public string ClaimValue { get; set; } = string.Empty;

    public RoleReadDbModel? Role { get; set; }
}
