using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserClaims;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.Mappers;

internal static class ApplicationUserMapper
{
    public static ApplicationUser ToDbModel(
        User user,
        UserName userName,
        UserStatus status,
        DateTime createdAt,
        DateTime updatedAt)
    {
        var dbModel = new ApplicationUser
        {
            Id = user.Id.Value,
            LockoutEnabled = true,
            CreatedAt = createdAt,
            Claims = ToClaimDbModels(
                userId: user.Id.Value,
                claims: user.Claims),
            Roles = ToRoleDbModels(user: user)
        };

        MapToDbModel(
            user: user,
            userName: userName,
            status: status,
            dbModel: dbModel,
            updatedAt: updatedAt);

        return dbModel;
    }

    public static void MapToDbModel(
        User user,
        UserName userName,
        UserStatus status,
        ApplicationUser dbModel,
        DateTime updatedAt)
    {
        dbModel.Email = user.Email.Value;
        dbModel.UserName = userName.Value;
        dbModel.EmailConfirmed = user.ConfirmationStatus.IsConfirmed;
        dbModel.Status = status.Name;
        dbModel.UpdatedAt = updatedAt;
    }

    public static List<ApplicationUserClaim> ToClaimDbModels(
        Guid userId,
        IEnumerable<UserClaim> claims)
    {
        return
        [
            .. claims.Select(claim => new ApplicationUserClaim
            {
                UserId = userId,
                ClaimType = claim.Type.Value,
                ClaimValue = claim.Value.Value
            })
        ];
    }

    public static Result<User> ToDomain(ApplicationUser user)
    {
        return User.Create(
            id: user.Id,
            email: user.Email!,
            confirmationStatus: user.EmailConfirmed,
            roleIds: user.Roles.Select(role => role.RoleId),
            claims: user.Claims.Select(claim => (claim.ClaimType!, claim.ClaimValue!)));
    }

    private static List<ApplicationUserRole> ToRoleDbModels(User user)
    {
        return
        [
            .. user.RoleIds.Select(roleId => new ApplicationUserRole
            {
                UserId = user.Id.Value,
                RoleId = roleId.Value
            })
        ];
    }
}
