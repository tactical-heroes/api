using PANiXiDA.TacticalHeroes.Identity.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Delete;

public sealed class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(command => command.Id)
            .MustBeValidDomainValue(UserId.Create);
    }
}
