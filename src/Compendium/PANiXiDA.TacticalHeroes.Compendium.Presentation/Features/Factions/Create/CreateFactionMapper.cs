using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Create;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.Create;

internal static class CreateFactionMapper
{
    internal static CreateFactionCommand ToCommand(CreateFactionRequest request)
    {
        return new CreateFactionCommand(
            Name: request.Name,
            Description: request.Description);
    }

    internal static CreateFactionResponse ToResponse(Guid id)
    {
        return new CreateFactionResponse(Id: id);
    }
}
