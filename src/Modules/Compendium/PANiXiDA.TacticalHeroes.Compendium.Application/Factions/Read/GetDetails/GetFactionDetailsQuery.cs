namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Read.GetDetails;

public sealed record GetFactionDetailsQuery(Guid Id)
    : IQuery<Result<FactionDetailsReadModel>>;
