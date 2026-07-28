using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Create;

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
