using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Delete;

public sealed class DeleteHeroCommandValidator : AbstractValidator<DeleteHeroCommand>
{
    public DeleteHeroCommandValidator()
    {
        RuleFor(expression: command => command.Id)
            .MustBeValidDomainValue(factory: HeroId.Create);
    }
}
