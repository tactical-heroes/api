using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserClaims;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
internal static partial class ApplicationUserMapper
{
    [MapProperty(
        "Id.Value",
        nameof(ApplicationUser.Id))]
    [MapProperty(
        "Email.Value",
        nameof(ApplicationUser.Email))]
    [MapProperty(
        "ConfirmationStatus.IsConfirmed",
        nameof(ApplicationUser.EmailConfirmed))]
    [MapPropertyFromSource(
        nameof(ApplicationUser.Claims),
        Use = nameof(ToClaimDbModelsFromUser))]
    [MapPropertyFromSource(
        nameof(ApplicationUser.Roles),
        Use = nameof(ToRoleDbModels))]
    [MapValue(
        nameof(ApplicationUser.LockoutEnabled),
        true)]
    public static partial ApplicationUser ToDbModel(
        User user,
        UserName userName,
        UserStatus status,
        DateTime createdAt,
        DateTime updatedAt);

    [MapperIgnore]
    public static void MapToDbModel(
        User user,
        UserName userName,
        UserStatus status,
        ApplicationUser dbModel,
        DateTime updatedAt)
    {
        MapToDbModel(
            source: new ApplicationUserUpdate(
                User: user,
                UserName: userName,
                Status: status,
                UpdatedAt: updatedAt),
            dbModel: dbModel);
    }

    [MapperIgnore]
    public static List<ApplicationUserClaim> ToClaimDbModels(
        Guid userId,
        IEnumerable<UserClaim> claims)
    {
        return
        [
            .. claims.Select(claim => ToClaimDbModel(
                claim: claim,
                userId: userId))
        ];
    }

    [MapperIgnore]
    public static Result<User> ToDomain(ApplicationUser user)
    {
        return User.Create(
            id: user.Id,
            email: user.Email!,
            confirmationStatus: user.EmailConfirmed,
            roleIds: user.Roles.Select(role => role.RoleId),
            claims: user.Claims.Select(claim => (claim.ClaimType!, claim.ClaimValue!)));
    }

    [MapProperty(
        "Type.Value",
        nameof(ApplicationUserClaim.ClaimType))]
    [MapProperty(
        "Value.Value",
        nameof(ApplicationUserClaim.ClaimValue))]
    [MapperIgnoreSource(nameof(UserClaim.Id))]
    private static partial ApplicationUserClaim ToClaimDbModel(
        UserClaim claim,
        Guid userId);

    [MapProperty(
        "User.Email.Value",
        nameof(ApplicationUser.Email))]
    [MapProperty(
        "User.ConfirmationStatus.IsConfirmed",
        nameof(ApplicationUser.EmailConfirmed))]
    private static partial void MapToDbModel(
        ApplicationUserUpdate source,
        [MappingTarget] ApplicationUser dbModel);

    private static ICollection<ApplicationUserClaim> ToClaimDbModelsFromUser(User user)
    {
        return ToClaimDbModels(
            userId: user.Id.Value,
            claims: user.Claims);
    }

    private static ICollection<ApplicationUserRole> ToRoleDbModels(User user)
    {
        return
        [
            .. user.RoleIds.Select(roleId => ToRoleDbModel(
                roleId: roleId,
                userId: user.Id.Value))
        ];
    }

    [MapProperty(
        "Value",
        nameof(ApplicationUserRole.RoleId))]
    private static partial ApplicationUserRole ToRoleDbModel(
        RoleId roleId,
        Guid userId);

    [UserMapping(Default = true)]
    private static string ToUserName(UserName userName) => userName.Value;

    [UserMapping(Default = true)]
    private static string ToUserStatus(UserStatus status) => status.Name;

    private sealed record ApplicationUserUpdate(
        User User,
        UserName UserName,
        UserStatus Status,
        DateTime UpdatedAt);
}
