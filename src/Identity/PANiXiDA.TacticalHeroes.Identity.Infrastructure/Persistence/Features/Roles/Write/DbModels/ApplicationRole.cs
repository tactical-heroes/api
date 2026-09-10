using Microsoft.AspNetCore.Identity;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;

public sealed class ApplicationRole : IdentityRole<Guid>
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ApplicationUserRole> Users { get; set; } = [];
    public ICollection<ApplicationRoleClaim> Claims { get; set; } = [];
}
