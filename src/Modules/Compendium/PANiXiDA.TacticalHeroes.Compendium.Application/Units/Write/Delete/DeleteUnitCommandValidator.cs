using PANiXiDA.TacticalHeroes.Compendium.Domain.Units;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Units.Write.Delete;

public sealed class DeleteUnitCommandValidator : AbstractValidator<DeleteUnitCommand>
{
    public DeleteUnitCommandValidator()
    {
        RuleFor(command => command.Id)
            .MustBeValidDomainValue(UnitId.Create);
    }
}
