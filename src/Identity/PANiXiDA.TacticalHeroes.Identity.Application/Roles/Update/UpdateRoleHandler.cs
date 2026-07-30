using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.Update;

public sealed class UpdateRoleHandler(IRolesWriteRepository rolesRepository)
    : ICommandHandler<UpdateRoleCommand, Result>
{
    public Task<Result> HandleAsync(
        UpdateRoleCommand command,
        CancellationToken cancellationToken)
    {
        return rolesRepository.UpdateAsync(
            id: command.Id,
            name: command.Name,
            claims: command.Claims,
            cancellationToken: cancellationToken);
    }
}
