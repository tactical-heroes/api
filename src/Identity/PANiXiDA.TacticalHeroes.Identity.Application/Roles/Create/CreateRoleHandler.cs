using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.Create;

public sealed class CreateRoleHandler(IRolesWriteRepository rolesRepository)
    : ICommandHandler<CreateRoleCommand, Result<Guid>>
{
    public Task<Result<Guid>> HandleAsync(
        CreateRoleCommand command,
        CancellationToken cancellationToken)
    {
        return rolesRepository.AddAsync(
            command.Name,
            command.Claims,
            cancellationToken);
    }
}
