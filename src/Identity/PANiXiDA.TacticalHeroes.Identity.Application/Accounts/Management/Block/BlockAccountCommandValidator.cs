using PANiXiDA.TacticalHeroes.Identity.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.Block;

public sealed class BlockAccountCommandValidator : AbstractValidator<BlockAccountCommand>
{
    public BlockAccountCommandValidator()
    {
        RuleFor(command => command.Id)
            .MustBeValidDomainValue(UserId.Create);
    }
}
