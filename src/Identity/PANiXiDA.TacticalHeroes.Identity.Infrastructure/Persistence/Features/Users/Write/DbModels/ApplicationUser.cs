using Microsoft.AspNetCore.Identity;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ApplicationUserRole> Roles { get; set; } = [];
    public ICollection<ApplicationUserClaim> Claims { get; set; } = [];
    public ICollection<ApplicationUserLogin> Logins { get; set; } = [];
}
