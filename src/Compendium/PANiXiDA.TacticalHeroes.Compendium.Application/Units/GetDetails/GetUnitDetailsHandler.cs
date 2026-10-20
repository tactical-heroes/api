using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;
using PANiXiDA.TacticalHeroes.Compendium.Application.Units.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Units.GetDetails;

public sealed class GetUnitDetailsHandler(
    IUnitsReadRepository unitsReadRepository)
    : IQueryHandler<GetUnitDetailsQuery, Result<UnitDetailsReadModel>>, IRequireAuthorization
{
    public static IReadOnlyCollection<string> AllPermissions { get; } = [Permissions.Compendium.Units.Read.Code];

    public async Task<Result<UnitDetailsReadModel>> HandleAsync(
        GetUnitDetailsQuery query,
        CancellationToken cancellationToken)
    {
        var unit = await unitsReadRepository.GetDetailsByIdAsync(
            id: query.Id,
            cancellationToken: cancellationToken);

        return unit is null
            ? Result.Failure<UnitDetailsReadModel>(
                error: Error.NotFound(message: "Unit was not found."))
            : Result.Success(value: unit);
    }
}
