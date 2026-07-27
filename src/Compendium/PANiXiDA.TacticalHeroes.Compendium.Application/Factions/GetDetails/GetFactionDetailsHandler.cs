using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetDetails;

public sealed class GetFactionDetailsHandler(
    IFactionsReadRepository factionsRepository)
    : IQueryHandler<GetFactionDetailsQuery, Result<FactionDetailsReadModel>>
{
    public async Task<Result<FactionDetailsReadModel>> HandleAsync(
        GetFactionDetailsQuery query,
        CancellationToken cancellationToken)
    {
        var faction = await factionsRepository.GetDetailsByIdAsync(
            id: query.Id,
            cancellationToken: cancellationToken);

        return faction is null
            ? Result.Failure<FactionDetailsReadModel>(
                error: Error.NotFound(message: "Faction was not found."))
            : Result.Success(value: faction);
    }
}
