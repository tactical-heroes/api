using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Write.Delete;

public sealed class DeleteHeroCommandValidator : AbstractValidator<DeleteHeroCommand>
{
    public DeleteHeroCommandValidator()
    {
        RuleFor(command => command.Id)
            .MustBeValidDomainValue(HeroId.Create);
    }
}
