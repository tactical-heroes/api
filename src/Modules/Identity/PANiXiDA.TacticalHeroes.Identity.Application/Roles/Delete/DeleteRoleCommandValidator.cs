using PANiXiDA.TacticalHeroes.Modules.Identity.Domain.Roles;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.Roles.Delete;

public sealed class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    public DeleteRoleCommandValidator()
    {
        RuleFor(command => command.Id)
            .MustBeValidDomainValue(RoleId.Create);
    }
}
