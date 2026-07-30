using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Update;

public sealed class UpdateFactionCommandValidator : AbstractValidator<UpdateFactionCommand>
{
    public UpdateFactionCommandValidator()
    {
        RuleFor(expression: command => command.Id)
            .MustBeValidDomainValue(factory: FactionId.Create);

        RuleFor(expression: command => command.Name)
            .MustBeValidDomainValue(factory: FactionName.Create);

        RuleFor(expression: command => command.Description)
            .MustBeValidDomainValue(factory: FactionDescription.Create);
    }
}
