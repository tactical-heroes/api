using Microsoft.AspNetCore.Identity;

using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string Status { get; set; } = AccountStatus.Active.Name;

    public ICollection<ApplicationUserRole> Roles { get; set; } = [];
    public ICollection<ApplicationUserClaim> Claims { get; set; } = [];
    public ICollection<ApplicationUserLogin> Logins { get; set; } = [];
}
