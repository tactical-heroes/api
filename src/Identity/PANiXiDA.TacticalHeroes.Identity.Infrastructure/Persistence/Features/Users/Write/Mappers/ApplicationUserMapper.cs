using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserClaims;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserClaims.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.Mappers;

internal static class ApplicationUserMapper
{
    public static ApplicationUser ToDbModel(
        User user,
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
            dbModel: dbModel,
            updatedAt: updatedAt);

        return dbModel;
    }

    public static void MapToDbModel(
        User user,
        ApplicationUser dbModel,
        DateTime updatedAt)
    {
        dbModel.Email = user.Email.Value;
        dbModel.UserName = user.UserName.Value;
        dbModel.EmailConfirmed = user.ConfirmationStatus.IsConfirmed;
        dbModel.Status = user.Status.Name;
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
        var idResult = UserId.Create(value: user.Id);
        var emailResult = Email.Create(value: user.Email!);
        var userNameResult = UserName.Create(value: user.UserName!);
        var statusResult = UserStatus.Create(value: user.Status);
        var validationResult = Result.Combine(idResult, emailResult, userNameResult, statusResult);

        if (validationResult.IsFailure)
        {
            return Result.Failure<User>(errors: validationResult.Errors);
        }

        var domainRoleIds = new List<RoleId>();

        foreach (var roleId in user.Roles.Select(role => role.RoleId))
        {
            var roleIdResult = RoleId.Create(value: roleId);

            if (roleIdResult.IsFailure)
            {
                return Result.Failure<User>(errors: roleIdResult.Errors);
            }

            domainRoleIds.Add(roleIdResult.Value);
        }

        var domainClaims = new List<UserClaim>();

        foreach (var claim in user.Claims)
        {
            var typeResult = ClaimType.Create(value: claim.ClaimType!);
            var valueResult = ClaimValue.Create(value: claim.ClaimValue!);
            var claimResult = Result.Combine(typeResult, valueResult);

            if (claimResult.IsFailure)
            {
                return Result.Failure<User>(errors: claimResult.Errors);
            }

            domainClaims.Add(UserClaim.Create(type: typeResult.Value, value: valueResult.Value));
        }

        return Result.Success(value: User.Create(
            id: idResult.Value,
            email: emailResult.Value,
            userName: userNameResult.Value,
            status: statusResult.Value,
            confirmationStatus: UserConfirmationStatus.From(isConfirmed: user.EmailConfirmed),
            roleIds: domainRoleIds,
            claims: domainClaims));
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
