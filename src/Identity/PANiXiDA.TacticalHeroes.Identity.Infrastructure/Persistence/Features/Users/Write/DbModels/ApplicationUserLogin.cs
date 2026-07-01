using Microsoft.AspNetCore.Identity;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

public sealed class ApplicationUserLogin : IdentityUserLogin<Guid>
{
    public ApplicationUser? User { get; set; }
}
