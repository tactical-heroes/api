using PANiXiDA.TacticalHeroes.Modules.Compendium.Domain.Factions;

namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Factions.Delete;

public sealed class DeleteFactionCommandValidator : AbstractValidator<DeleteFactionCommand>
{
    public DeleteFactionCommandValidator()
    {
        RuleFor(command => command.Id)
            .MustBeValidDomainValue(FactionId.Create);
    }
}
