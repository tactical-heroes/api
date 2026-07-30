using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.Delete;

public sealed class DeleteRoleHandler(IRolesWriteRepository rolesRepository)
    : ICommandHandler<DeleteRoleCommand, Result>
{
    public Task<Result> HandleAsync(
        DeleteRoleCommand command,
        CancellationToken cancellationToken)
    {
        return rolesRepository.DeleteAsync(command.Id, cancellationToken);
    }
}
