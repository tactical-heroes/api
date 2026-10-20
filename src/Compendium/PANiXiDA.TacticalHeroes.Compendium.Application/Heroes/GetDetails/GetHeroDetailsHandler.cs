using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;
using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetDetails;

public sealed class GetHeroDetailsHandler(
    IHeroesReadRepository heroesReadRepository)
    : IQueryHandler<GetHeroDetailsQuery, Result<HeroDetailsReadModel>>, IRequireAuthorization
{
    public static IReadOnlyCollection<string> AllPermissions { get; } =
    [
        Permissions.Heroes.Read.Code
    ];

    public async Task<Result<HeroDetailsReadModel>> HandleAsync(
        GetHeroDetailsQuery query,
        CancellationToken cancellationToken)
    {
        var hero = await heroesReadRepository.GetDetailsByIdAsync(
            id: query.Id,
            cancellationToken: cancellationToken);

        return hero is null
            ? Result.Failure<HeroDetailsReadModel>(
                error: Error.NotFound(message: "Hero was not found."))
            : Result.Success(value: hero);
    }
}
