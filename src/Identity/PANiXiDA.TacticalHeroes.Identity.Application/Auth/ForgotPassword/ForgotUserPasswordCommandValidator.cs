using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.ForgotPassword;

public sealed class ForgotUserPasswordCommandValidator : AbstractValidator<ForgotUserPasswordCommand>
{
    public ForgotUserPasswordCommandValidator()
    {
        RuleFor(command => command.Email)
            .MustBeValidDomainValue(Email.Create);
    }
}
