using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.Create;

public sealed class CreateRoleHandler(IRolesWriteRepository rolesRepository)
    : ICommandHandler<CreateRoleCommand, Result<Guid>>
{
    public Task<Result<Guid>> HandleAsync(
        CreateRoleCommand command,
        CancellationToken cancellationToken)
    {
        var roleResult = RoleMapper.ToDomain(
            id: RoleId.New().Value,
            name: command.Name,
            claims: command.Claims.Select(claim => (claim.Type, claim.Value)));

        return roleResult.IsFailure
            ? Task.FromResult(Result.Failure<Guid>(errors: roleResult.Errors))
            : rolesRepository.AddAsync(
                role: roleResult.Value,
                cancellationToken: cancellationToken);
    }
}
