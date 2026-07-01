using Microsoft.AspNetCore.Identity;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;

public sealed class ApplicationRoleClaim : IdentityRoleClaim<Guid>
{
    public ApplicationRole? Role { get; set; }
}
