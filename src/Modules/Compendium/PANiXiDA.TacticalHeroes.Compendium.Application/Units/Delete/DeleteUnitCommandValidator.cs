using PANiXiDA.TacticalHeroes.Modules.Compendium.Domain.Units;

namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Units.Delete;

public sealed class DeleteUnitCommandValidator : AbstractValidator<DeleteUnitCommand>
{
    public DeleteUnitCommandValidator()
    {
        RuleFor(command => command.Id)
            .MustBeValidDomainValue(UnitId.Create);
    }
}
