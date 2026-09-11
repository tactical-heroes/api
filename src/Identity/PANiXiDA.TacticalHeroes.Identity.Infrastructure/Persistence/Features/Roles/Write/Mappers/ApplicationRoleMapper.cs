using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Entities.RoleClaims;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Entities.RoleClaims.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.Mappers;

internal static class ApplicationRoleMapper
{
    public static ApplicationRole ToDbModel(
        Role role,
        DateTime createdAt,
        DateTime updatedAt)
    {
        var dbModel = new ApplicationRole
        {
            Id = role.Id.Value,
            CreatedAt = createdAt,
            Claims = ToClaimDbModels(claims: role.Claims)
        };

        MapToDbModel(
            role: role,
            dbModel: dbModel,
            updatedAt: updatedAt);

        return dbModel;
    }

    public static void MapToDbModel(
        Role role,
        ApplicationRole dbModel,
        DateTime updatedAt)
    {
        dbModel.Name = role.Name.Value;
        dbModel.UpdatedAt = updatedAt;
    }

    public static List<ApplicationRoleClaim> ToClaimDbModels(
        IEnumerable<RoleClaim> claims)
    {
        return
        [
            .. claims.Select(claim => new ApplicationRoleClaim
            {
                ClaimType = claim.Type.Value,
                ClaimValue = claim.Value.Value
            })
        ];
    }

    public static Result<Role> ToDomain(ApplicationRole role)
    {
        var idResult = RoleId.Create(value: role.Id);
        var nameResult = RoleName.Create(value: role.Name!);
        var validationResult = Result.Combine(idResult, nameResult);

        if (validationResult.IsFailure)
        {
            return Result.Failure<Role>(errors: validationResult.Errors);
        }

        var domainClaims = new List<RoleClaim>();

        foreach (var claim in role.Claims)
        {
            var typeResult = ClaimType.Create(value: claim.ClaimType!);
            var valueResult = ClaimValue.Create(value: claim.ClaimValue!);
            var claimResult = Result.Combine(typeResult, valueResult);

            if (claimResult.IsFailure)
            {
                return Result.Failure<Role>(errors: claimResult.Errors);
            }

            domainClaims.Add(RoleClaim.Create(type: typeResult.Value, value: valueResult.Value));
        }

        return Result.Success(value: Role.Create(
            id: idResult.Value,
            name: nameResult.Value,
            claims: domainClaims));
    }
}
