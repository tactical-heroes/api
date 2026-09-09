using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Entities.RoleClaims;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Entities.RoleClaims.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles;

public static class RoleMapper
{
    public static Result<Role> ToDomain(
        Guid id,
        string name,
        IEnumerable<(string Type, string Value)> claims)
    {
        var idResult = RoleId.Create(value: id);
        var nameResult = RoleName.Create(value: name);
        var validationResult = Result.Combine(idResult, nameResult);

        if (validationResult.IsFailure)
        {
            return Result.Failure<Role>(errors: validationResult.Errors);
        }

        var domainClaims = new List<RoleClaim>();

        foreach (var claim in claims)
        {
            var typeResult = ClaimType.Create(value: claim.Type);
            var valueResult = ClaimValue.Create(value: claim.Value);
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
