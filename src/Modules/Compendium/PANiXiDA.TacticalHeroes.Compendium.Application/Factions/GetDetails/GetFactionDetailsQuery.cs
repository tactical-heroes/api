namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Factions.GetDetails;

public sealed record GetFactionDetailsQuery(Guid Id)
    : IQuery<Result<FactionDetailsReadModel>>;
