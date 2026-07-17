using PANiXiDA.TacticalHeroes.Identity.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Confirm;

public sealed class ConfirmAccountCommandValidator : AbstractValidator<ConfirmAccountCommand>
{
    public ConfirmAccountCommandValidator()
    {
        RuleFor(command => command.AccountId)
            .MustBeValidDomainValue(UserId.Create);

        RuleFor(command => command.EmailConfirmationToken)
            .NotEmpty();
    }
}
