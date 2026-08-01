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

        RuleFor(command => command.Attack)
            .GreaterThanOrEqualTo(0);

        RuleFor(command => command.Defense)
            .GreaterThanOrEqualTo(0);

        RuleFor(command => command.Health)
            .GreaterThan(0);

        RuleFor(command => command.MinimumDamage)
            .GreaterThanOrEqualTo(0);

        RuleFor(command => command.MaximumDamage)
            .GreaterThanOrEqualTo(command => command.MinimumDamage);

        RuleFor(command => command.Initiative)
            .Must(value => double.IsFinite(value) && value >= 0);

        RuleFor(command => command.Speed)
            .GreaterThanOrEqualTo(0);

        RuleFor(command => command.Shots)
            .GreaterThan(0)
            .When(command => command.Shots.HasValue);

        RuleFor(command => command.RangedAttackRange)
            .GreaterThan(0)
            .When(command => command.RangedAttackRange.HasValue);

        RuleFor(command => command)
            .Must(command =>
                command.Shots.HasValue == command.RangedAttackRange.HasValue)
            .WithMessage(
                "Shots and ranged attack range must both be provided or both be omitted.")
            .OverridePropertyName(nameof(UpdateUnitCommand.RangedAttackRange));

        RuleFor(command => command.Morale)
            .InclusiveBetween(UnitMorale.Minimum, UnitMorale.Maximum);

        RuleFor(command => command.Luck)
            .InclusiveBetween(UnitLuck.Minimum, UnitLuck.Maximum);

        RuleFor(command => command.FactionId)
            .MustBeValidDomainValue(FactionId.Create);
    }
}
