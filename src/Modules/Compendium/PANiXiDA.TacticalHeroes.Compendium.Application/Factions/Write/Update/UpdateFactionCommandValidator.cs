using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Write.Update;

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
