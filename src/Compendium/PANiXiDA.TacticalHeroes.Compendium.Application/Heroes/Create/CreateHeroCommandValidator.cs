using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Create;

public sealed class CreateHeroCommandValidator : AbstractValidator<CreateHeroCommand>
{
    public CreateHeroCommandValidator()
    {
        RuleFor(command => command.Name)
            .MustBeValidDomainValue(HeroName.Create);

        RuleFor(command => command.Description)
            .MustBeValidDomainValue(HeroDescription.Create);

        RuleFor(command => command)
            .MustBeValidDomainResult(command => HeroCombatStats.Create(
                attack: command.Attack,
                defense: command.Defense,
                minimumDamage: command.MinimumDamage,
                maximumDamage: command.MaximumDamage,
                initiative: command.Initiative));

        RuleFor(command => command.Morale)
            .MustBeValidDomainValue(HeroMorale.Create);

        RuleFor(command => command.Luck)
            .MustBeValidDomainValue(HeroLuck.Create);

        RuleFor(command => command.FactionId)
            .MustBeValidDomainValue(FactionId.Create);
    }
}
