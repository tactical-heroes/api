using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;
using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Entities.RoleClaims;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Entities.RoleClaims.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.Create;

public sealed class CreateRoleHandler(
    IRolesRepository rolesRepository,
    IRolesReadRepository rolesReadRepository)
    : ICommandHandler<CreateRoleCommand, Result<Guid>>, IRequireAuthorization
{
    public static IReadOnlyCollection<string> AllPermissions { get; } = [Permissions.Identity.Roles.Create.Code];

    public async Task<Result<Guid>> HandleAsync(
        CreateRoleCommand command,
        CancellationToken cancellationToken)
    {
        var nameResult = RoleName.Create(value: command.Name);
        var defaultFlagResult = RoleDefaultFlag.Create(value: command.IsDefault);
        var validationResult = Result.Combine(nameResult, defaultFlagResult);

        if (validationResult.IsFailure)
        {
            return Result.Failure<Guid>(errors: validationResult.Errors);
        }

        var claims = new List<RoleClaim>();

        foreach (var claim in command.Claims)
        {
            var typeResult = ClaimType.Create(value: claim.Type);
            var valueResult = ClaimValue.Create(value: claim.Value);
            var claimResult = Result.Combine(typeResult, valueResult);

            if (claimResult.IsFailure)
            {
                return Result.Failure<Guid>(errors: claimResult.Errors);
            }

            claims.Add(RoleClaim.Create(type: typeResult.Value, value: valueResult.Value));
        }

        if (command.IsDefault)
        {
            var defaultRoleId = await rolesReadRepository.GetDefaultIdAsync(cancellationToken);

            if (defaultRoleId is not null)
            {
                return Result.Failure<Guid>(
                    error: Error.Conflict(message: "Default role already exists."));
            }
        }

        var role = Role.Create(
            id: RoleId.New(),
            name: nameResult.Value,
            defaultFlag: defaultFlagResult.Value,
            claims: claims);

        return await rolesRepository.AddAsync(
            role: role,
            cancellationToken: cancellationToken);
    }
}
