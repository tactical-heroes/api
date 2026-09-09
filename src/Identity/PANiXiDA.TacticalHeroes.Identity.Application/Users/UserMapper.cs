using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserClaims;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserClaims.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users;

public static class UserMapper
{
    public static Result<User> ToDomain(
        Guid id,
        string email,
        bool isConfirmed,
        IEnumerable<Guid> roleIds,
        IEnumerable<(string Type, string Value)> claims)
    {
        var idResult = UserId.Create(value: id);
        var emailResult = Email.Create(value: email);
        var validationResult = Result.Combine(idResult, emailResult);

        if (validationResult.IsFailure)
        {
            return Result.Failure<User>(errors: validationResult.Errors);
        }

        var domainRoleIds = new List<RoleId>();

        foreach (var roleId in roleIds)
        {
            var roleIdResult = RoleId.Create(value: roleId);

            if (roleIdResult.IsFailure)
            {
                return Result.Failure<User>(errors: roleIdResult.Errors);
            }

            domainRoleIds.Add(roleIdResult.Value);
        }

        var domainClaims = new List<UserClaim>();

        foreach (var claim in claims)
        {
            var typeResult = ClaimType.Create(value: claim.Type);
            var valueResult = ClaimValue.Create(value: claim.Value);
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
            confirmationStatus: UserConfirmationStatus.From(isConfirmed: isConfirmed),
            roleIds: domainRoleIds,
            claims: domainClaims));
    }
}
