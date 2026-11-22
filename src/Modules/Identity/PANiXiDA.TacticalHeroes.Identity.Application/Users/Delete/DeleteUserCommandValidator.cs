using PANiXiDA.TacticalHeroes.Modules.Identity.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.Users.Delete;

public sealed class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(command => command.Id)
            .MustBeValidDomainValue(UserId.Create);
    }
}
