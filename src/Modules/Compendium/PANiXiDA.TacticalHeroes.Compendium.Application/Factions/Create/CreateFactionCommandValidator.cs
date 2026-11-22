using PANiXiDA.TacticalHeroes.Modules.Compendium.Domain.Factions.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Factions.Create;

public sealed class CreateFactionCommandValidator : AbstractValidator<CreateFactionCommand>
{
    public CreateFactionCommandValidator()
    {
        RuleFor(command => command.Name)
            .MustBeValidDomainValue(FactionName.Create);

        RuleFor(command => command.Description)
            .MustBeValidDomainValue(FactionDescription.Create);
    }
}
