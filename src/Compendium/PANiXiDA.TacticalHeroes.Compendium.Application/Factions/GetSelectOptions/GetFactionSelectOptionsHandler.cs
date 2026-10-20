using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetSelectOptions;

public sealed class GetFactionSelectOptionsHandler(IFactionsReadRepository factionsReadRepository) :
    IQueryHandler<GetFactionSelectOptionsQuery, Result<IReadOnlyList<FactionSelectOptionReadModel>>>,
    IRequireAuthorization
{
    public static IReadOnlyCollection<string> AnyPermissions { get; } =
    [
        Permissions.Factions.Read.Code,
        Permissions.Heroes.Create.Code,
        Permissions.Heroes.Update.Code,
        Permissions.Units.Create.Code,
        Permissions.Units.Update.Code
    ];

    public async Task<Result<IReadOnlyList<FactionSelectOptionReadModel>>> HandleAsync(
        GetFactionSelectOptionsQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<FactionSelectOptionReadModel> options = await factionsReadRepository.GetSelectOptionsAsync(
            filter: query.Filter,
            limit: query.Limit,
            cancellationToken: cancellationToken);

        return Result.Success(value: options);
    }
}
