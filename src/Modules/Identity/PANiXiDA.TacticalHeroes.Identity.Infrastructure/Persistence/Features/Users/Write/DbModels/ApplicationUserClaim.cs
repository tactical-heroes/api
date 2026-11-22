using Microsoft.AspNetCore.Identity;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

public sealed class ApplicationUserClaim : IdentityUserClaim<Guid>
{
    public ApplicationUser? User { get; set; }
}
