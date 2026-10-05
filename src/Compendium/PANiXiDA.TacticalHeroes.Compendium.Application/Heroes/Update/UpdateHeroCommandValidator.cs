using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Update;

public sealed class UpdateHeroCommandValidator : AbstractValidator<UpdateHeroCommand>
{
    public UpdateHeroCommandValidator()
    {
        RuleFor(expression: command => command.Id)
            .MustBeValidDomainValue(factory: HeroId.Create);

        RuleFor(expression: command => command.Name)
            .MustBeValidDomainValue(factory: HeroName.Create);

        RuleFor(expression: command => command.Description)
            .MustBeValidDomainValue(factory: HeroDescription.Create);

        RuleFor(expression: command => command)
            .MustBeValidDomainResult(factory: command => HeroCombatStats.Create(
                attack: command.Attack,
                defense: command.Defense,
                minimumDamage: command.MinimumDamage,
                maximumDamage: command.MaximumDamage,
                initiative: command.Initiative));

        RuleFor(expression: command => command.Morale)
            .MustBeValidDomainValue(factory: HeroMorale.Create);

        RuleFor(expression: command => command.Luck)
            .MustBeValidDomainValue(factory: HeroLuck.Create);

        RuleFor(expression: command => command.FactionId)
            .MustBeValidDomainValue(factory: FactionId.Create);
    }
}
