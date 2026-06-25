using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.PasswordReset;

public sealed class RequestPasswordResetCommandValidator : AbstractValidator<RequestPasswordResetCommand>
{
    public RequestPasswordResetCommandValidator()
    {
        RuleFor(command => command.Email)
            .MustBeValidDomainValue(Email.Create);
    }
}
