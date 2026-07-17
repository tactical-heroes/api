using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserClaims.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.Create;

public sealed class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(command => command.Email)
            .MustBeValidDomainValue(Email.Create);

        RuleFor(command => command.UserName)
            .MustBeValidDomainValue(UserName.Create);

        RuleFor(command => command.Password)
            .NotEmpty();

        RuleFor(command => command.Claims)
            .NotNull();

        RuleForEach(command => command.Claims)
            .ChildRules(claim =>
            {
                claim.RuleFor(item => item.Type)
                    .MustBeValidDomainValue(ClaimType.Create);

                claim.RuleFor(item => item.Value)
                    .MustBeValidDomainValue(ClaimValue.Create);
            });

        RuleFor(command => command.Status)
            .MustBeValidDomainValue(AccountStatus.Create);
    }
}
