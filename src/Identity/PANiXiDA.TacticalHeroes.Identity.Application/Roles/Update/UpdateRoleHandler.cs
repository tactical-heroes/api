using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Entities.RoleClaims;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Entities.RoleClaims.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.Update;

public sealed class UpdateRoleHandler(IRolesRepository rolesRepository)
    : ICommandHandler<UpdateRoleCommand, Result>
{
    public Task<Result> HandleAsync(
        UpdateRoleCommand command,
        CancellationToken cancellationToken)
    {
        var idResult = RoleId.Create(value: command.Id);
        var nameResult = RoleName.Create(value: command.Name);
        var validationResult = Result.Combine(idResult, nameResult);

        if (validationResult.IsFailure)
        {
            return Task.FromResult(Result.Failure(errors: validationResult.Errors));
        }

        var claims = new List<RoleClaim>();

        foreach (var claim in command.Claims)
        {
            var typeResult = ClaimType.Create(value: claim.Type);
            var valueResult = ClaimValue.Create(value: claim.Value);
            var claimResult = Result.Combine(typeResult, valueResult);

            if (claimResult.IsFailure)
            {
                return Task.FromResult(Result.Failure(errors: claimResult.Errors));
            }

            claims.Add(RoleClaim.Create(type: typeResult.Value, value: valueResult.Value));
        }

        var role = Role.Create(
            id: idResult.Value,
            name: nameResult.Value,
            claims: claims);

        return rolesRepository.UpdateAsync(
            role: role,
            cancellationToken: cancellationToken);
    }
}
