using System.ComponentModel.DataAnnotations;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;

public sealed class UserClaimReadDbModel : ReadDbModel<int>
{
    public Guid UserId { get; set; }

    [MaxLength(256)]
    public string ClaimType { get; set; } = string.Empty;

    [MaxLength(1024)]
    public string ClaimValue { get; set; } = string.Empty;

    public UserReadDbModel? User { get; set; }
}
