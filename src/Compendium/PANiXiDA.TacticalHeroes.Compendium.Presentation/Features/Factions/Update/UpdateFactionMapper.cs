using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Update;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.Update;

internal static class UpdateFactionMapper
{
    internal static UpdateFactionCommand ToCommand(
        UpdateFactionRequest request,
        Guid id)
    {
        return new UpdateFactionCommand(
            Id: id,
            Name: request.Name,
            Description: request.Description);
    }
}
