using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Entities.RoleClaims;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal static partial class ApplicationRoleMapper
{
    [MapProperty(
        "Id.Value",
        nameof(ApplicationRole.Id))]
    [MapProperty(
        "Name.Value",
        nameof(ApplicationRole.Name))]
    [MapperIgnoreTarget(nameof(ApplicationRole.ConcurrencyStamp))]
    [MapperIgnoreTarget(nameof(ApplicationRole.NormalizedName))]
    [MapperIgnoreTarget(nameof(ApplicationRole.Users))]
    public static partial ApplicationRole ToDbModel(
        Role role,
        DateTime createdAt,
        DateTime updatedAt);

    [MapProperty(
        "Name.Value",
        nameof(ApplicationRole.Name))]
    [MapperIgnoreTarget(nameof(ApplicationRole.Claims))]
    [MapperIgnoreTarget(nameof(ApplicationRole.ConcurrencyStamp))]
    [MapperIgnoreTarget(nameof(ApplicationRole.CreatedAt))]
    [MapperIgnoreTarget(nameof(ApplicationRole.Id))]
    [MapperIgnoreTarget(nameof(ApplicationRole.NormalizedName))]
    [MapperIgnoreTarget(nameof(ApplicationRole.Users))]
    public static partial void MapToDbModel(
        Role role,
        [MappingTarget] ApplicationRole dbModel,
        DateTime updatedAt);

    public static partial List<ApplicationRoleClaim> ToClaimDbModels(
        IEnumerable<RoleClaim> claims);

    [MapperIgnore]
    public static Result<Role> ToDomain(ApplicationRole role)
    {
        return Role.Create(
            id: role.Id,
            name: role.Name!,
            claims: role.Claims.Select(claim => (claim.ClaimType!, claim.ClaimValue!)));
    }

    [MapProperty(
        "Type.Value",
        nameof(ApplicationRoleClaim.ClaimType))]
    [MapProperty(
        "Value.Value",
        nameof(ApplicationRoleClaim.ClaimValue))]
    [MapperIgnoreTarget(nameof(ApplicationRoleClaim.Id))]
    [MapperIgnoreTarget(nameof(ApplicationRoleClaim.Role))]
    [MapperIgnoreTarget(nameof(ApplicationRoleClaim.RoleId))]
    private static partial ApplicationRoleClaim ToClaimDbModel(RoleClaim claim);
}
