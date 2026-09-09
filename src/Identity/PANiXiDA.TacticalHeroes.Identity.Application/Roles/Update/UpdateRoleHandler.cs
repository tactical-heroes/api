using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.Update;

public sealed class UpdateRoleHandler(IRolesWriteRepository rolesRepository)
    : ICommandHandler<UpdateRoleCommand, Result>
{
    public Task<Result> HandleAsync(
        UpdateRoleCommand command,
        CancellationToken cancellationToken)
    {
        var roleResult = RoleMapper.ToDomain(
            id: command.Id,
            name: command.Name,
            claims: command.Claims.Select(claim => (claim.Type, claim.Value)));

        return roleResult.IsFailure
            ? Task.FromResult(Result.Failure(errors: roleResult.Errors))
            : rolesRepository.UpdateAsync(
                role: roleResult.Value,
                cancellationToken: cancellationToken);
    }
}
