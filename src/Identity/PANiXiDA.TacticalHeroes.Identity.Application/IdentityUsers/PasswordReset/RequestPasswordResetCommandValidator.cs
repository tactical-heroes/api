using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Application.IdentityUsers.PasswordReset;

public sealed class RequestPasswordResetCommandValidator : AbstractValidator<RequestPasswordResetCommand>
{
    public RequestPasswordResetCommandValidator()
    {
        RuleFor(command => command.Email)
            .MustBeValidDomainValue(Email.Create);
    }
}
