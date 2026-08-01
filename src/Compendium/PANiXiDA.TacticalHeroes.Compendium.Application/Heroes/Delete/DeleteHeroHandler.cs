using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Delete;

public sealed class DeleteHeroHandler(IHeroesRepository heroesRepository)
    : ICommandHandler<DeleteHeroCommand, Result>
{
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

        await heroesRepository.DeleteAsync(
            aggregateRoot: hero,
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}
