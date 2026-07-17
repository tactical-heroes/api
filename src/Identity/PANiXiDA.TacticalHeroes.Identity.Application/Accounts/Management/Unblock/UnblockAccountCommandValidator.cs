using PANiXiDA.TacticalHeroes.Identity.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.Unblock;

public sealed class UnblockAccountCommandValidator : AbstractValidator<UnblockAccountCommand>
{
    public UnblockAccountCommandValidator()
    {
        RuleFor(command => command.Id)
            .MustBeValidDomainValue(UserId.Create);
    }
}
