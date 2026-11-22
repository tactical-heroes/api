using PANiXiDA.TacticalHeroes.Modules.Compendium.Domain.Heroes;

namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Heroes.Delete;

public sealed class DeleteHeroCommandValidator : AbstractValidator<DeleteHeroCommand>
{
    public DeleteHeroCommandValidator()
    {
        RuleFor(command => command.Id)
            .MustBeValidDomainValue(HeroId.Create);
    }
}
