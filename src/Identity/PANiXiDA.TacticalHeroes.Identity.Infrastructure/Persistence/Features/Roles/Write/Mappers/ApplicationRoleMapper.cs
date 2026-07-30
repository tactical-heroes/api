using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Entities.RoleClaims;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
internal static partial class ApplicationRoleMapper
{
    [MapProperty(
        "Id.Value",
        nameof(ApplicationRole.Id))]
    [MapProperty(
        "Name.Value",
        nameof(ApplicationRole.Name))]
    public static partial ApplicationRole ToDbModel(
        Role role,
        DateTime createdAt,
        DateTime updatedAt);

    [MapProperty(
        "Name.Value",
        nameof(ApplicationRole.Name))]
    [MapperIgnoreSource(nameof(Role.Claims))]
    [MapperIgnoreSource(nameof(Role.Id))]
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
    [MapperIgnoreSource(nameof(RoleClaim.Id))]
    private static partial ApplicationRoleClaim ToClaimDbModel(RoleClaim claim);
}
