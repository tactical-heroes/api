using Microsoft.AspNetCore.Identity;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

public sealed class ApplicationUserRole : IdentityUserRole<Guid>
{
    public ApplicationUser? User { get; set; }
    public ApplicationRole? Role { get; set; }
}
