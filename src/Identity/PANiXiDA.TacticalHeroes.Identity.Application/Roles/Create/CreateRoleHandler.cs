using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Entities.RoleClaims;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Entities.RoleClaims.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.Create;

public sealed class CreateRoleHandler(IRolesRepository rolesRepository)
    : ICommandHandler<CreateRoleCommand, Result<Guid>>
{
    public Task<Result<Guid>> HandleAsync(
        CreateRoleCommand command,
        CancellationToken cancellationToken)
    {
        var nameResult = RoleName.Create(value: command.Name);
        var validationResult = Result.Combine(nameResult);

        if (validationResult.IsFailure)
        {
            return Task.FromResult(Result.Failure<Guid>(errors: validationResult.Errors));
        }

        var claims = new List<RoleClaim>();

        foreach (var claim in command.Claims)
        {
            var typeResult = ClaimType.Create(value: claim.Type);
            var valueResult = ClaimValue.Create(value: claim.Value);
            var claimResult = Result.Combine(typeResult, valueResult);

            if (claimResult.IsFailure)
            {
                return Task.FromResult(Result.Failure<Guid>(errors: claimResult.Errors));
            }

            claims.Add(RoleClaim.Create(type: typeResult.Value, value: valueResult.Value));
        }

        var role = Role.Create(
            id: RoleId.New(),
            name: nameResult.Value,
            claims: claims);

        return rolesRepository.AddAsync(
            role: role,
            cancellationToken: cancellationToken);
    }
}
