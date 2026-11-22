using PANiXiDA.TacticalHeroes.Modules.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Modules.Compendium.Domain.Factions.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Factions.Update;

public sealed class UpdateFactionCommandValidator : AbstractValidator<UpdateFactionCommand>
{
    public UpdateFactionCommandValidator()
    {
        RuleFor(command => command.Id)
            .MustBeValidDomainValue(FactionId.Create);

        RuleFor(command => command.Name)
            .MustBeValidDomainValue(FactionName.Create);

        RuleFor(command => command.Description)
            .MustBeValidDomainValue(FactionDescription.Create);
    }
}
