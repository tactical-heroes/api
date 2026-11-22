namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetDetails;

public sealed record GetFactionDetailsQuery(Guid Id)
    : IQuery<Result<FactionDetailsReadModel>>;
