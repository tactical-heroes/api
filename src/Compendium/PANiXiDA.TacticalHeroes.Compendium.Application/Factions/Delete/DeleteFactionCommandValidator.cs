using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Delete;

public sealed class DeleteFactionCommandValidator : AbstractValidator<DeleteFactionCommand>
{
    public DeleteFactionCommandValidator()
    {
        RuleFor(expression: command => command.Id)
            .MustBeValidDomainValue(factory: FactionId.Create);
    }
}
