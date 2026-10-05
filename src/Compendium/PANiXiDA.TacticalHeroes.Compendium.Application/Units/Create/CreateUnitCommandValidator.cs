using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Units.Create;

public sealed class CreateUnitCommandValidator : AbstractValidator<CreateUnitCommand>
{
    public CreateUnitCommandValidator()
    {
        RuleFor(expression: command => command.Name)
            .MustBeValidDomainValue(factory: UnitName.Create);

        RuleFor(expression: command => command.Description)
            .MustBeValidDomainValue(factory: UnitDescription.Create);

        RuleFor(expression: command => command)
            .MustBeValidDomainResult(factory: command => UnitCombatStats.Create(
                attack: command.Attack,
                defense: command.Defense,
                health: command.Health,
                minimumDamage: command.MinimumDamage,
                maximumDamage: command.MaximumDamage,
                initiative: command.Initiative,
                speed: command.Speed));

        RuleFor(expression: command => command)
            .MustBeValidDomainResult(factory: command => UnitRangedAttack.Create(
                shots: command.Shots,
                rangedAttackRange: command.RangedAttackRange));

        RuleFor(expression: command => command.Morale)
            .MustBeValidDomainValue(factory: UnitMorale.Create);

        RuleFor(expression: command => command.Luck)
            .MustBeValidDomainValue(factory: UnitLuck.Create);

        RuleFor(expression: command => command.FactionId)
            .MustBeValidDomainValue(factory: FactionId.Create);
    }
}
