using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Delete;

public sealed class DeleteHeroHandler(IHeroesRepository heroesRepository)
    : ICommandHandler<DeleteHeroCommand, Result>, IRequireAuthorization, IRequireTransaction
{
    public static IReadOnlyCollection<string> AllPermissions { get; } = [Permissions.Compendium.Heroes.Delete.Code];

    public async Task<Result> HandleAsync(
        DeleteHeroCommand command,
        CancellationToken cancellationToken)
    {
        var idResult = HeroId.Create(value: command.Id);

        if (idResult.IsFailure)
        {
            return Result.Failure(errors: idResult.Errors);
        }

        var hero = await heroesRepository.GetByIdAsync(
            id: idResult.Value,
            cancellationToken: cancellationToken);

        if (hero is null)
        {
            return Result.Failure(
                error: Error.NotFound(message: "Hero was not found."));
        }

        heroesRepository.Delete(aggregateRoot: hero);

        return Result.Success();
    }
}
