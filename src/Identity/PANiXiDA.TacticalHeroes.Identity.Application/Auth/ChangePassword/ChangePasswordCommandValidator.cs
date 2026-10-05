using PANiXiDA.TacticalHeroes.Identity.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.ChangePassword;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(expression: command => command.UserId)
            .MustBeValidDomainValue(factory: UserId.Create);

        RuleFor(expression: command => command.CurrentPassword)
            .NotEmpty();

        RuleFor(expression: command => command.NewPassword)
            .NotEmpty();
    }
}
