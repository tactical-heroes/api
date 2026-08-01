using PANiXiDA.TacticalHeroes.Compendium.Application.Validation;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Units.Update;

public sealed class UpdateUnitCommandValidator : AbstractValidator<UpdateUnitCommand>
{
    public UpdateUnitCommandValidator()
    {
        RuleFor(command => command.Id)
            .MustBeValidDomainValue(UnitId.Create);

        RuleFor(command => command.Name)
            .MustBeValidDomainValue(UnitName.Create);

        RuleFor(command => command.Description)
            .MustBeValidDomainValue(UnitDescription.Create);

        RuleFor(command => command)
            .MustBeValidDomainResult(command => UnitCombatStats.Create(
                attack: command.Attack,
                defense: command.Defense,
                health: command.Health,
                minimumDamage: command.MinimumDamage,
                maximumDamage: command.MaximumDamage,
                initiative: command.Initiative,
                speed: command.Speed,
                shots: command.Shots,
                rangedAttackRange: command.RangedAttackRange));

        RuleFor(command => command.Morale)
            .MustBeValidDomainValue(UnitMorale.Create);

        RuleFor(command => command.Luck)
            .MustBeValidDomainValue(UnitLuck.Create);

        RuleFor(command => command.FactionId)
            .MustBeValidDomainValue(FactionId.Create);
    }
}
