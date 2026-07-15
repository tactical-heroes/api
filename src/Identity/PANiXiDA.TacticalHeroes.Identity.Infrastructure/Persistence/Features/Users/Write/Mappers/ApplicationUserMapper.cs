using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.Mappers;

internal static class ApplicationUserMapper
{
    public static Result<User> ToDomain(ApplicationUser user)
    {
        return User.Create(
            user.Id,
            user.Email!,
            user.EmailConfirmed,
            user.Roles.Select(role => role.RoleId),
            user.Claims.Select(claim => (claim.ClaimType!, claim.ClaimValue!)));
    }
}
