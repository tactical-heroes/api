using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.Write.Delete;

public sealed class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    public DeleteRoleCommandValidator()
    {
        RuleFor(command => command.Id)
            .MustBeValidDomainValue(RoleId.Create);
    }
}
