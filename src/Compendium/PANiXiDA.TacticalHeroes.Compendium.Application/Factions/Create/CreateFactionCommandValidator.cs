using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Create;

public sealed class CreateFactionCommandValidator : AbstractValidator<CreateFactionCommand>
{
    public CreateFactionCommandValidator()
    {
        RuleFor(expression: command => command.Name)
            .MustBeValidDomainValue(factory: FactionName.Create);

        RuleFor(expression: command => command.Description)
            .MustBeValidDomainValue(factory: FactionDescription.Create);
    }
}
