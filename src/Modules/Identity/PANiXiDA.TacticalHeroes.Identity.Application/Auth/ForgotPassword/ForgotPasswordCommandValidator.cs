using PANiXiDA.TacticalHeroes.Modules.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.Auth.ForgotPassword;

public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(command => command.Email)
            .MustBeValidDomainValue(Email.Create);
    }
}
