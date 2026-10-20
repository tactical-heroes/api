using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetList;

public sealed class GetFactionsHandler(IFactionsReadRepository factionsReadRepository)
    : IQueryHandler<GetFactionsQuery, Result<PaginationResult<FactionListItemReadModel>>>, IRequireAuthorization
{
    public static IReadOnlyCollection<string> AllPermissions { get; } = [Permissions.Compendium.Factions.Read.Code];

    public async Task<Result<PaginationResult<FactionListItemReadModel>>> HandleAsync(
        GetFactionsQuery query,
        CancellationToken cancellationToken)
    {
        var factions = await factionsReadRepository.GetPageAsync(
            paginationParameters: query.PaginationParameters,
            sortingParameters: query.SortingParameters,
            cancellationToken: cancellationToken);

        return Result.Success(value: factions);
    }
}
