using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Delete;

public sealed class DeleteFactionCommandValidator : AbstractValidator<DeleteFactionCommand>
{
    public DeleteFactionCommandValidator()
    {
        RuleFor(command => command.Id)
            .MustBeValidDomainValue(FactionId.Create);
    }
}
