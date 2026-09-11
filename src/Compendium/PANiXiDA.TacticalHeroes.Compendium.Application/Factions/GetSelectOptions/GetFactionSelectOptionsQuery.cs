namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetSelectOptions;

public sealed record GetFactionSelectOptionsQuery(string? Search, int Limit)
    : IQuery<Result<IReadOnlyList<FactionSelectOptionReadModel>>>;
