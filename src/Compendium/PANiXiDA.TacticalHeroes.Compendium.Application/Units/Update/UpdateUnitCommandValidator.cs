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
                input: command.ToUnitAttributes().CombatStats));

        RuleFor(command => command.Morale)
            .MustBeValidDomainValue(UnitMorale.Create);

        RuleFor(command => command.Luck)
            .MustBeValidDomainValue(UnitLuck.Create);

        RuleFor(command => command.FactionId)
            .MustBeValidDomainValue(FactionId.Create);
    }
}
