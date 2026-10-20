using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetDetails;

public sealed class GetFactionDetailsHandler(
    IFactionsReadRepository factionsReadRepository)
    : IQueryHandler<GetFactionDetailsQuery, Result<FactionDetailsReadModel>>, IRequireAuthorization
{
    public static IReadOnlyCollection<string> AllPermissions { get; } = [Permissions.Factions.Read.Code];

    public async Task<Result<FactionDetailsReadModel>> HandleAsync(
        GetFactionDetailsQuery query,
        CancellationToken cancellationToken)
    {
        var faction = await factionsReadRepository.GetDetailsByIdAsync(
            id: query.Id,
            cancellationToken: cancellationToken);

        return faction is null
            ? Result.Failure<FactionDetailsReadModel>(
                error: Error.NotFound(message: "Faction was not found."))
            : Result.Success(value: faction);
    }
}
