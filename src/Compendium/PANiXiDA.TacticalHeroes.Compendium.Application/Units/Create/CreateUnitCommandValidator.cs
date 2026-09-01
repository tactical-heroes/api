using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Units.Create;

public sealed class CreateUnitCommandValidator : AbstractValidator<CreateUnitCommand>
{
    public CreateUnitCommandValidator()
    {
        RuleFor(command => command.Name)
            .MustBeValidDomainValue(UnitName.Create);

        RuleFor(command => command.Description)
            .MustBeValidDomainValue(UnitDescription.Create);

        RuleFor(command => command)
            .MustBeValidDomainResult(command => UnitCombatStats.Create(
                input: command.ToUnitAttributes().CombatStats));

        RuleFor(command => command.Morale)
            .MustBeValidDomainValue(UnitMorale.Create);

        RuleFor(command => command.Luck)
            .MustBeValidDomainValue(UnitLuck.Create);

        RuleFor(command => command.FactionId)
            .MustBeValidDomainValue(FactionId.Create);
    }
}
