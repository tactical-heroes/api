using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Update;

public sealed class UpdateHeroHandler(
    IHeroesRepository heroesRepository,
    IFactionsRepository factionsRepository)
    : ICommandHandler<UpdateHeroCommand, Result>
{
    public async Task<Result> HandleAsync(
        UpdateHeroCommand command,
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

        var factionIdResult = FactionId.Create(value: command.FactionId);

        if (factionIdResult.IsFailure)
        {
            return Result.Failure(errors: factionIdResult.Errors);
        }

        var faction = await factionsRepository.GetByIdAsync(
            id: factionIdResult.Value,
            cancellationToken: cancellationToken);

        if (faction is null)
        {
            return Result.Failure(
                error: Error.NotFound(message: "Faction was not found."));
        }

        var updateResult = hero.Update(
            name: command.Name,
            description: command.Description,
            attack: command.Attack,
            defense: command.Defense,
            minimumDamage: command.MinimumDamage,
            maximumDamage: command.MaximumDamage,
            initiative: command.Initiative,
            morale: command.Morale,
            luck: command.Luck,
            factionId: command.FactionId);

        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        await heroesRepository.UpdateAsync(
            aggregateRoot: hero,
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}
